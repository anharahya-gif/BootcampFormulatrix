using PokerAPIMPwDB.Domain.Interfaces;
using PokerAPIMPwDB.Domain.Enums;
using PokerAPIMPwDB.Domain.Models;
using PokerAPIMPwDB.Infrastructure.Persistence;
using PokerAPIMPwDB.DTO.Player;
using PokerAPIMPwDB.Common.Results;
using Microsoft.EntityFrameworkCore;
using PokerAPIMPwDB.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace PokerAPIMPwDB.Domain.GameEngine
{
    // PokerGameEngine per table (tidak shared antar table)
    public class PokerGameEngine : IPokerGameEngine
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<PokerHub> _hub;

        private List<IPlayer> _players = new();
        private int _currentPlayerIndex = 0;
        private int _currentBet = 0;
        private List<ICard> _communityCards = new();
        private ShowdownResult? _lastShowdown;

        public Guid CurrentTableId { get; internal set; } = Guid.Empty;

        public GamePhase Phase { get; private set; } = GamePhase.WaitingForPlayer;
        public int CurrentBet => _currentBet;
        public int CurrentPlayerIndex => _currentPlayerIndex;
        public List<ICard> CommunityCards => _communityCards;
        public ShowdownResult? LastShowdown => _lastShowdown;

        // Events untuk broadcast ke clients
        public event Action? RoundStarted;
        public event Action? CommunityCardsUpdated;
        public event Action? ShowdownCompleted;

        public PokerGameEngine(AppDbContext db, IHubContext<PokerHub> hub)
        {
            _db = db;
            _hub = hub;

            // Event bindings untuk broadcasting
            RoundStarted += async () =>
            {
                await _hub.Clients.Group(CurrentTableId.ToString()).SendAsync("RoundStarted", new
                {
                    Phase = Phase.ToString(),
                    Players = GetPlayersPublicState()
                });
            };

            CommunityCardsUpdated += async () =>
            {
                await _hub.Clients.Group(CurrentTableId.ToString()).SendAsync("CommunityCardsUpdated", CommunityCards);
            };

            ShowdownCompleted += async () =>
            {
                await _hub.Clients.Group(CurrentTableId.ToString()).SendAsync("ShowdownCompleted", GetShowdownDetails());
            };
        }

        #region Load Players
        public async Task LoadPlayersFromTableAsync(Guid tableId)
        {
            CurrentTableId = tableId;

            var table = await _db.Tables
                .Include(t => t.PlayerSeats)
                .ThenInclude(ps => ps.Player)
                .FirstOrDefaultAsync(t => t.Id == tableId);

            _players.Clear();
            foreach (var seat in table.PlayerSeats)
            {
                var domainPlayer = MapToDomain(seat.Player);
                _players.Add(domainPlayer);
            }
        }
        #endregion

        #region Game State
        public string GetGameState() => Phase.ToString();
        public bool CanStartRound() => _players.Count >= 2;
        public int GetTotalPot() => _players.Sum(p => p.CurrentBet);
        public IPlayer? GetPlayerByName(string name) => _players.FirstOrDefault(p => p.DisplayName == name);
        public int GetTotalPlayers() => _players.Count;

        public IEnumerable<PlayerPublicStateDto> GetPlayersPublicState() =>
            _players.Select(p => new PlayerPublicStateDto
            {
                DisplayName = p.DisplayName,
                ChipStack = p.ChipStack,
                CurrentBet = p.CurrentBet,
                State = p.State
            });
        #endregion

        #region Player Management
        public ServiceResult AddPlayer(string name, int chips, int seatIndex, Guid playerId)
        {
            try
            {
                if (_players.Any(p => p.DisplayName == name))
                    return ServiceResult.Fail("Player with same name already exists");

                // Tambahkan PlayerId ke domain model
                var domainPlayer = new Domain.Models.Player
                {
                    PlayerId = playerId,        // pakai parameter
                    DisplayName = name,
                    ChipStack = chips,
                    CurrentBet = 0,
                    State = PlayerState.Active,
                    SeatIndex = seatIndex
                };

                _players.Add(domainPlayer);

                // Simpan ke DB, gunakan PlayerId juga
                var entityPlayer = new Infrastructure.Persistence.Entities.Player
                {
                    UserId = playerId,            // pakai parameter
                    DisplayName = domainPlayer.DisplayName,
                    ChipStack = domainPlayer.ChipStack,
                    State = domainPlayer.State,
                    PlayerSeat = new Infrastructure.Persistence.Entities.PlayerSeat
                    {
                        Id = Guid.NewGuid(),
                        SeatNumber = domainPlayer.SeatIndex
                    }
                };

                _db.Players.Add(entityPlayer);
                _db.SaveChanges();

                return ServiceResult.Success("Player added successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail($"Error adding player: {ex.Message}");
            }
        }


        public ServiceResult RemovePlayer(IPlayer player)
        {
            try
            {
                if (!_players.Contains(player))
                    return ServiceResult.Fail("Player not found in game");

                _players.Remove(player);

                var dbPlayer = _db.Players
                    .Include(p => p.PlayerSeat)
                    .FirstOrDefault(p => p.UserId == ((Domain.Models.Player)player).PlayerId);

                if (dbPlayer != null)
                {
                    if (dbPlayer.PlayerSeat != null)
                        _db.PlayerSeats.Remove(dbPlayer.PlayerSeat);
                    _db.Players.Remove(dbPlayer);
                }

                _db.SaveChanges();
                return ServiceResult.Success("Player removed successfully");
            }
            catch (Exception ex)
            {
                return ServiceResult.Fail($"Error removing player: {ex.Message}");
            }
        }

        public List<IPlayer> ActivePlayers() => _players.Where(p => p.State == PlayerState.Active).ToList();
        #endregion

        #region Round Management
        public ServiceResult StartRound()
        {
            if (!CanStartRound())
                return ServiceResult.Fail("Not enough players to start round");

            Phase = GamePhase.PreFlop;
            _currentPlayerIndex = 0;
            _currentBet = 0;
            _communityCards.Clear();

            foreach (var p in _players)
            {
                p.CurrentBet = 0;
                p.State = PlayerState.Active;

                var dbPlayer = _db.Players.FirstOrDefault(x => x.UserId == ((Domain.Models.Player)p).PlayerId);
                if (dbPlayer != null)
                    dbPlayer.State = PlayerState.Active;
            }

            _db.SaveChanges();
            RoundStarted?.Invoke();
            return ServiceResult.Success("Round started");
        }

        public ServiceResult NextPhase()
        {
            Phase = Phase switch
            {
                GamePhase.PreFlop => GamePhase.Flop,
                GamePhase.Flop => GamePhase.Turn,
                GamePhase.Turn => GamePhase.River,
                GamePhase.River => GamePhase.Showdown,
                GamePhase.Showdown => GamePhase.WaitingForPlayer,
                _ => Phase
            };

            CommunityCardsUpdated?.Invoke();
            return ServiceResult.Success("Phase advanced");
        }
        #endregion

        #region Player Actions
        public IPlayer GetCurrentPlayer() => _players[_currentPlayerIndex];

        public IPlayer GetNextActivePlayer()
        {
            int start = _currentPlayerIndex;
            do
            {
                _currentPlayerIndex = (_currentPlayerIndex + 1) % _players.Count;
                if (_players[_currentPlayerIndex].State == PlayerState.Active)
                    return _players[_currentPlayerIndex];
            } while (_currentPlayerIndex != start);

            return _players[_currentPlayerIndex];
        }

        public bool IsBettingRoundOver() => _players.All(p => p.CurrentBet == _currentBet || p.State != PlayerState.Active);

        public ServiceResult<int> HandleBet(IPlayer player, int amount)
        {
            if (player == null) return ServiceResult<int>.Fail("Player not found");
            if (player.ChipStack < amount) return ServiceResult<int>.Fail("Not enough chips");

            player.ChipStack -= amount;
            player.CurrentBet += amount;
            if (player.CurrentBet > _currentBet) _currentBet = player.CurrentBet;

            var dbPlayer = _db.Players.FirstOrDefault(p => p.UserId == ((Domain.Models.Player)player).PlayerId);
            if (dbPlayer != null) dbPlayer.ChipStack = player.ChipStack;

            _db.SaveChanges();
            return ServiceResult<int>.Success(_currentBet, "Bet accepted");
        }

        public ServiceResult<int> HandleCall(IPlayer player) => HandleBet(player, _currentBet - player.CurrentBet);

        public ServiceResult<int> HandleRaise(IPlayer player, int raiseAmount)
        {
            var callResult = HandleCall(player);
            if (!callResult.IsSuccess) return ServiceResult<int>.Fail(callResult.Message);

            return HandleBet(player, raiseAmount);
        }

        public ServiceResult HandleFold(IPlayer player)
        {
            player.State = PlayerState.Folded;

            var dbPlayer = _db.Players.FirstOrDefault(p => p.UserId == ((Domain.Models.Player)player).PlayerId);
            if (dbPlayer != null) dbPlayer.State = player.State;

            _db.SaveChanges();
            return ServiceResult.Success("Player folded");
        }

        public ServiceResult HandleCheck(IPlayer player)
        {
            player.State = PlayerState.Active; // tetap aktif
            return ServiceResult.Success("Player checked");
        }

        public ServiceResult HandleAllIn(string playerName)
        {
            var player = GetPlayerByName(playerName);
            if (player == null) return ServiceResult.Fail("Player not found");
            if (player.ChipStack <= 0) return ServiceResult.Fail("No chips left");

            int allInAmount = player.ChipStack;
            player.CurrentBet += allInAmount;
            player.ChipStack = 0;
            player.State = PlayerState.AllIn;

            if (player.CurrentBet > _currentBet)
                _currentBet = player.CurrentBet;

            var dbPlayer = _db.Players.FirstOrDefault(p => p.UserId == ((Domain.Models.Player)player).PlayerId);
            if (dbPlayer != null)
            {
                dbPlayer.ChipStack = player.ChipStack;
                dbPlayer.State = player.State;
            }

            _db.SaveChanges();
            return ServiceResult.Success("Player is all-in");
        }
        #endregion

        #region Showdown & Hand Evaluation
        public Dictionary<IPlayer, HandRank> EvaluateHands()
        {
            var result = new Dictionary<IPlayer, HandRank>();
            foreach (var player in _players.Where(p => p.State != PlayerState.Folded))
            {
                var fullHand = player.Cards.Concat(_communityCards).ToList();
                result[player] = EvaluateHand(fullHand);
            }
            return result;
        }

        public List<IPlayer> DetermineWinners()
        {
            var hands = EvaluateHands();
            if (!hands.Any()) return new List<IPlayer>();

            var bestRankValue = hands.Max(h => (int)h.Value);
            return hands.Where(h => (int)h.Value == bestRankValue).Select(h => h.Key).ToList();
        }

        public List<IPlayer> ResolveShowdown()
        {
            var winners = DetermineWinners();
            int totalPot = GetTotalPot();
            int share = totalPot / Math.Max(1, winners.Count);

            foreach (var winner in winners)
            {
                winner.ChipStack += share;
                var dbPlayer = _db.Players.FirstOrDefault(p => p.UserId == ((Domain.Models.Player)winner).PlayerId);
                if (dbPlayer != null) dbPlayer.ChipStack = winner.ChipStack;
            }

            _db.SaveChanges();
            ShowdownCompleted?.Invoke();
            return winners;
        }

        public (List<IPlayer> winners, HandRank rank) ResolveShowdownDetailed()
        {
            var winners = DetermineWinners();
            var hands = EvaluateHands();
            HandRank bestRank = winners.Any() ? hands[winners.First()] : HandRank.HighCard;
            return (winners, bestRank);
        }

        public object? EvaluateVisibleForPlayer(string playerName)
        {
            var player = GetPlayerByName(playerName);
            if (player == null) return null;

            return new
            {
                Player = player.DisplayName,
                Hand = player.Cards.ToList(),
                CommunityCards = _communityCards
            };
        }

        public object GetShowdownDetails()
        {
            var hands = EvaluateHands();
            return new
            {
                CommunityCards = _communityCards.Select(c => new { c.Rank, c.Suit }).ToList(),
                Players = _players.Select(p => new
                {
                    Player = p.DisplayName,
                    State = p.State,
                    ChipStack = p.ChipStack,
                    HoleCards = p.Cards.Select(c => new { c.Rank, c.Suit }).ToList(),
                    HandRank = hands.ContainsKey(p) ? (HandRank?)hands[p] : null
                }).ToList()
            };
        }

        private HandRank EvaluateHand(List<ICard> cards)
        {
            if (cards == null || cards.Count == 0)
                return HandRank.HighCard;

            var rankGroups = cards.GroupBy(c => c.Rank)
                                  .Select(g => new { Rank = g.Key, Count = g.Count() })
                                  .OrderByDescending(g => g.Count)
                                  .ThenByDescending(g => g.Rank)
                                  .ToList();

            var suitGroups = cards.GroupBy(c => c.Suit)
                                  .Where(g => g.Count() >= 5)
                                  .ToList();
            bool isFlush = suitGroups.Any();

            var orderedRanks = cards.Select(c => (int)c.Rank).Distinct().OrderBy(r => r).ToList();
            bool isStraight = false;
            for (int i = 0; i <= orderedRanks.Count - 5; i++)
            {
                if (orderedRanks[i + 4] - orderedRanks[i] == 4)
                {
                    isStraight = true;
                    break;
                }
            }

            if (isFlush && isStraight)
            {
                var flushCards = suitGroups.First().Select(c => (int)c.Rank).Distinct().OrderBy(r => r).ToList();
                for (int i = 0; i <= flushCards.Count - 5; i++)
                {
                    if (flushCards[i + 4] - flushCards[i] == 4)
                    {
                        if (flushCards[i + 4] == (int)Rank.Ace)
                            return HandRank.RoyalFlush;
                        return HandRank.StraightFlush;
                    }
                }
            }

            if (rankGroups[0].Count == 4)
                return HandRank.FourOfAKind;
            if (rankGroups[0].Count == 3 && rankGroups.Count > 1 && rankGroups[1].Count >= 2)
                return HandRank.FullHouse;
            if (isFlush)
                return HandRank.Flush;
            if (isStraight)
                return HandRank.Straight;
            if (rankGroups[0].Count == 3)
                return HandRank.ThreeOfAKind;
            if (rankGroups[0].Count == 2 && rankGroups.Count > 1 && rankGroups[1].Count == 2)
                return HandRank.TwoPair;
            if (rankGroups[0].Count == 2)
                return HandRank.OnePair;

            return HandRank.HighCard;
        }
        #endregion

        #region Mapping Helpers
        public static Domain.Models.Player MapToDomain(Infrastructure.Persistence.Entities.Player entity)
        {
            return new Domain.Models.Player
            {
                PlayerId = entity.UserId,
                DisplayName = entity.DisplayName,
                ChipStack = entity.ChipStack,
                State = entity.State,
                SeatIndex = entity.PlayerSeat?.SeatNumber ?? -1,
                CurrentBet = 0
            };
        }
        #endregion
    }
}
