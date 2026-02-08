using PokerAPIMPwDB.Domain.Interfaces;
using PokerAPIMPwDB.Domain.Enums;
using PokerAPIMPwDB.Domain.Models;
using PokerAPIMPwDB.Infrastructure.Persistence;
using PokerAPIMPwDB.DTO.Player;
using PokerAPIMPwDB.DTO.Table;
using PokerAPIMPwDB.Common.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using PokerAPIMPwDB.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerAPIMPwDB.Domain.GameEngine
{
    public class PokerGameEngine : IPokerGameEngine
    {
        private readonly AppDbContext _db;
        private readonly IHubContext<PokerHub> _hub;

        // =========================
        // CORE STATE (SEAT-CENTRIC)
        // =========================
        private readonly List<PlayerSeat> _seats = new();
        private int _currentPlayerIndex;
        private int _currentBet;
        private readonly List<ICard> _communityCards = new();
        private ShowdownResult? _lastShowdown;

        private readonly List<IPlayer> _players = new();

        public Guid CurrentTableId { get; internal set; } = Guid.Empty;

        public GamePhase Phase { get; private set; } = GamePhase.WaitingForPlayer;
        public int CurrentBet => _currentBet;
        public int CurrentPlayerIndex => _currentPlayerIndex;
        public List<ICard> CommunityCards => _communityCards;
        public ShowdownResult? LastShowdown => _lastShowdown;
        public IServiceScope? Scope { get; set; }

        // =========================
        // EVENTS
        // =========================
        public event Func<Task>? RoundStarted;
        public event Func<Task>? CommunityCardsUpdated;
        public event Func<Task>? ShowdownCompleted;
        public PokerGameEngine(AppDbContext db, IHubContext<PokerHub> hub)
        {
            _db = db;
            _hub = hub;
        }

        // =========================
        // LOAD TABLE / SEATS
        // =========================
        public async Task LoadPlayersFromTableAsync(Guid tableId)
        {
            CurrentTableId = tableId;

            var table = await _db.Tables
                .Include(t => t.PlayerSeats)
                .ThenInclude(ps => ps.Player)
                .FirstOrDefaultAsync(t => t.Id == tableId);

            if (table == null)
                throw new InvalidOperationException("Table not found");

            _seats.Clear();
            _players.Clear();

            // Map seats + player
            foreach (var ps in table.PlayerSeats.OrderBy(s => s.SeatNumber))
            {
                var seat = new PlayerSeat(ps.SeatNumber);

                if (ps.Player != null)
                {
                    var domainPlayer = MapToDomain(ps.Player);
                    seat.SitDown(domainPlayer, domainPlayer.ChipStack);
                    _players.Add(domainPlayer);
                }

                _seats.Add(seat);
            }

            // Debug: tampilkan seat yang tersedia
            Console.WriteLine("Loaded seats: " + string.Join(", ", _seats.Select(s => s.SeatIndex)));
        }


        // =========================
        // HELPERS
        // =========================

        private PlayerSeat? FindSeat(Guid playerId) =>
            _seats.FirstOrDefault(s => s.IsOccupied && s.Player!.PlayerId == playerId);

        private IEnumerable<PlayerSeat> ActiveSeats() =>
            _seats.Where(s => s.IsOccupied && s.Player!.State == PlayerState.Active);

        public IReadOnlyList<SeatStateDto> GetSeatsState()
        {
            return _seats.Select(seat => new SeatStateDto
            {
                SeatIndex = seat.SeatIndex,
                IsOccupied = seat.IsOccupied,
                PlayerId = seat.Player?.PlayerId,
                PlayerName = seat.Player?.DisplayName,
                Chips = seat.Player?.ChipStack ?? 0,
                IsFolded = seat.Player?.State == PlayerState.Folded,
                IsAllIn = seat.Player?.State == PlayerState.AllIn
            }).ToList();
        }

        public IEnumerable<PlayerPublicStateDto> GetPlayersPublicState() =>
            _seats.Where(s => s.IsOccupied)
                  .Select(s => s.Player!)
                  .Select(p => new PlayerPublicStateDto
                  {
                      DisplayName = p.DisplayName,
                      ChipStack = p.ChipStack,
                      CurrentBet = p.CurrentBet,
                      State = p.State
                  });
        // =========================
        // PLAYER MANAGEMENT (Async DB-backed)
        // =========================

        // =========================
        // LOBBY / JOIN TABLE
        // =========================
        public async Task<ServiceResult<TableStateDto>> JoinTableAsync(Guid tableId)
        {
            // Load state table & seats
            await LoadPlayersFromTableAsync(tableId);

            var tableState = new TableStateDto
            {
                TableId = tableId,
                Phase = Phase,
                CurrentBet = CurrentBet,
                CommunityCards = CommunityCards
                            .Select(c => new Card(c.Rank, c.Suit)) // fix constructor
                            .ToList(),
                // Masukkan seat state
                Seats = GetSeatsState()
            };


            // Map each seat
            foreach (var seat in _seats.OrderBy(s => s.SeatIndex))
            {
                tableState.Players.Add(new PlayerPublicStateDto
                {
                    PlayerId = seat.Player?.PlayerId ?? Guid.Empty,
                    DisplayName = seat.Player?.DisplayName ?? "",
                    ChipStack = seat.Player?.ChipStack ?? 0,
                    CurrentBet = seat.Player?.CurrentBet ?? 0,
                    State = seat.Player?.State ?? PlayerState.Waiting,
                    SeatIndex = seat.SeatIndex
                });
            }

            return ServiceResult<TableStateDto>.Success(tableState, "Table loaded");
        }
        // SitDown 

        public async Task<ServiceResult> SitDownAsync(Guid userId, string displayName, int seatIndex, int chips)
        {
            if (chips <= 0) return ServiceResult.Fail("Chip amount must be positive");

            // Cari seat
            var seat = _seats.FirstOrDefault(s => s.SeatIndex == seatIndex);
            if (seat == null) return ServiceResult.Fail("Seat does not exist");
            if (seat.IsOccupied) return ServiceResult.Fail("Seat is already occupied");

            // Buat player baru
            var player = new Domain.Models.Player
            {
                PlayerId = userId,
                DisplayName = displayName,
                ChipStack = chips,
                CurrentBet = 0,
                State = PlayerState.Active,
                SeatIndex = seatIndex
            };

            // Tambahkan ke memory game
            seat.SitDown(player, chips);
            _players.Add(player);

            // Tambahkan ke DB
            var dbPlayer = new Infrastructure.Persistence.Entities.Player
            {
                UserId = userId,
                DisplayName = displayName,
                ChipStack = chips,
                State = PlayerState.Active
            };
            _db.Players.Add(dbPlayer);

            // Update PlayerSeat di DB
            var dbSeat = await _db.PlayerSeats.FirstOrDefaultAsync(ps => ps.SeatNumber == seatIndex && ps.TableId == CurrentTableId);
            if (dbSeat != null)
            {
                dbSeat.PlayerId = userId; // Assign seat ke player baru
            }

            await _db.SaveChangesAsync();

            // Trigger event agar UI update
            if (RoundStarted != null) await RoundStarted.Invoke();

            return ServiceResult.Success("Player sat down successfully");
        }

        // StandUp / leave seat (tetap di table)
        public async Task<ServiceResult> StandUpAsync(Guid userId)
        {
            var seat = _seats.FirstOrDefault(s => s.IsOccupied && s.Player!.PlayerId == userId);
            if (seat == null)
                return ServiceResult.Fail("Player is not seated");

            var player = seat.Player!;

            // Lepas seat di memory
            seat.Leave();
            _players.Remove(player);

            // Hapus player dari DB
            var dbPlayer = await _db.Players.FindAsync(userId);
            if (dbPlayer != null)
                _db.Players.Remove(dbPlayer);

            // Hapus seat dari DB
            var dbSeat = await _db.PlayerSeats
                .FirstOrDefaultAsync(ps => ps.PlayerId == userId && ps.TableId == CurrentTableId);
            if (dbSeat != null)
                _db.PlayerSeats.Remove(dbSeat);

            await _db.SaveChangesAsync();

            // Push update via SignalR
            if (RoundStarted != null)
                await RoundStarted.Invoke();

            return ServiceResult.Success("Player stood up and removed from table");
        }
        // =========================
        // LEAVE TABLE
        // =========================
        public async Task<ServiceResult> LeaveTableAsync(Guid userId)
        {
            // Stand up dulu jika player duduk
            var player = _players.FirstOrDefault(p => p.PlayerId == userId);
            if (player != null)
            {
                var standResult = await StandUpAsync(userId);
                if (!standResult.IsSuccess)
                    return standResult;
            }

            // Hanya unload dari memory, player sudah dihapus via StandUpAsync
            return ServiceResult.Success("Player left table");
        }


        public List<IPlayer> ActivePlayers() => ActiveSeats().Select(s => s.Player!).ToList();
        // =========================
        // ROUND MANAGEMENT
        // =========================
        public ServiceResult StartRound()
        {
            if (_seats.Count(s => s.IsOccupied) < 2)
                return ServiceResult.Fail("Not enough players to start round");

            Phase = GamePhase.PreFlop;
            _currentBet = 0;
            _currentPlayerIndex = 0;
            _communityCards.Clear();

            foreach (var seat in ActiveSeats())
            {
                seat.Player!.CurrentBet = 0;
                seat.Player.State = PlayerState.Active;
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

        // =========================
        // PLAYER TURN MANAGEMENT
        // =========================
        public IPlayer GetCurrentPlayer() => _seats[_currentPlayerIndex].Player!;

        public IPlayer GetNextActivePlayer()
        {
            int startIndex = _currentPlayerIndex;
            do
            {
                _currentPlayerIndex = (_currentPlayerIndex + 1) % _seats.Count;
            } while (!_seats[_currentPlayerIndex].IsOccupied || _seats[_currentPlayerIndex].Player!.State != PlayerState.Active);

            return _seats[_currentPlayerIndex].Player!;
        }

        public bool IsBettingRoundOver()
        {
            var activePlayers = ActiveSeats().Select(s => s.Player!).ToList();
            return activePlayers.All(p => p.CurrentBet == _currentBet || p.State != PlayerState.Active);
        }

        // =========================
        // BETTING ACTIONS
        // =========================
        public ServiceResult<int> HandleBet(IPlayer player, int amount)
        {
            if (player.ChipStack < amount)
                return ServiceResult<int>.Fail("Not enough chips");

            player.ChipStack -= amount;
            player.CurrentBet += amount;

            if (player.CurrentBet > _currentBet)
                _currentBet = player.CurrentBet;

            _db.SaveChanges();
            return ServiceResult<int>.Success(_currentBet);
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
            _db.SaveChanges();
            return ServiceResult.Success("Folded");
        }

        public ServiceResult HandleCheck(IPlayer player) => ServiceResult.Success("Checked");

        public ServiceResult HandleAllIn(string playerName)
        {
            var player = _players.FirstOrDefault(p => p.DisplayName == playerName);
            if (player == null) return ServiceResult.Fail("Player not found");

            int amount = player.ChipStack;
            player.ChipStack = 0;
            player.CurrentBet += amount;
            player.State = PlayerState.AllIn;

            if (player.CurrentBet > _currentBet)
                _currentBet = player.CurrentBet;

            _db.SaveChanges();
            return ServiceResult.Success("All-in");
        }

        // =========================
        // SHOWDOWN
        // =========================
        public Dictionary<IPlayer, HandRank> EvaluateHands()
        {
            var result = new Dictionary<IPlayer, HandRank>();
            foreach (var player in ActivePlayers())
            {
                var combined = new List<ICard>();
                combined.AddRange(player.Cards);
                combined.AddRange(_communityCards);
                result[player] = EvaluateHand(combined);
            }
            return result;
        }


        public List<IPlayer> DetermineWinners()
        {
            var evaluated = EvaluateHands();
            if (!evaluated.Any()) return new List<IPlayer>();

            var maxRank = evaluated.Max(kv => kv.Value);
            return evaluated.Where(kv => kv.Value == maxRank).Select(kv => kv.Key).ToList();
        }

        public List<IPlayer> ResolveShowdown()
        {
            var winners = DetermineWinners();
            foreach (var winner in winners)
            {
                winner.ChipStack += GetTotalPot() / winners.Count;
            }

            _lastShowdown = new ShowdownResult
            {
                Winners = winners,
                CommunityCards = _communityCards.ToList(),
                PlayerHands = _players.ToDictionary(p => p, p => p.Cards.ToList())
            };

            ShowdownCompleted?.Invoke();
            return winners;
        }

        public (List<IPlayer> winners, HandRank rank) ResolveShowdownDetailed()
        {
            var hands = EvaluateHands();
            if (!hands.Any()) return (new List<IPlayer>(), HandRank.HighCard);

            var maxRank = hands.Max(kv => kv.Value);
            var winners = hands.Where(kv => kv.Value == maxRank).Select(kv => kv.Key).ToList();
            return (winners, maxRank);
        }

        public int GetTotalPot() => _players.Sum(p => p.CurrentBet);

        public object? EvaluateVisibleForPlayer(string playerName)
        {
            var player = _players.FirstOrDefault(p => p.DisplayName == playerName);
            if (player == null) return null;

            // Combine hole cards + community cards
            var combined = player.Cards.Concat(_communityCards).ToList();

            // Gunakan evaluator internal
            var rank = EvaluateHand(combined);

            return new
            {
                Player = player.DisplayName,
                HandRank = rank,
                HoleCards = player.Cards,
                CommunityCards = _communityCards
            };
        }


        public object GetShowdownDetails() => (object)(_lastShowdown ?? new ShowdownResult
        {
            Winners = new List<IPlayer>(),
            BestHandRank = HandRank.HighCard
        });


        // =========================
        // PLAYER LOOKUP
        // =========================
        public IPlayer? GetPlayerByName(string name) => _players.FirstOrDefault(p => p.DisplayName == name);
        public int GetTotalPlayers() => _players.Count;

        // =========================
        // GAME STATE STRING
        // =========================
        public string GetGameState() =>
            $"Phase: {Phase}, CurrentBet: {_currentBet}, TotalPlayers: {_players.Count}, Pot: {GetTotalPot()}";

        public bool CanStartRound() => _seats.Count(s => s.IsOccupied) >= 2;

        // =========================
        // MAPPING
        // =========================
        public static Domain.Models.Player MapToDomain(Infrastructure.Persistence.Entities.Player entity)
        {
            return new Domain.Models.Player
            {
                PlayerId = entity.UserId,
                DisplayName = entity.DisplayName,
                ChipStack = entity.ChipStack,
                CurrentBet = 0,
                State = entity.State,
                SeatIndex = 0, // nanti diassign dari seat
                Cards = new List<ICard>() // kosong dulu
            };
        }
        // =========================
        // HAND EVALUATOR (INTERNAL)
        // =========================
        // internal evaluator untuk list card arbitrary
        private HandRank EvaluateHand(List<ICard> cards)
        {
            if (cards.Count < 5) return HandRank.HighCard;

            var rankGroups = cards.GroupBy(c => c.Rank)
                                  .OrderByDescending(g => g.Count())
                                  .ToList();

            var suitGroups = cards.GroupBy(c => c.Suit)
                                  .OrderByDescending(g => g.Count())
                                  .ToList();

            bool isFlush = suitGroups[0].Count() >= 5;

            var orderedRanks = cards.Select(c => (int)c.Rank).Distinct().OrderBy(r => r).ToList();
            bool isStraight = false;
            for (int i = 0; i <= orderedRanks.Count - 5; i++)
                if (orderedRanks[i + 4] - orderedRanks[i] == 4) isStraight = true;

            if (isFlush && isStraight)
                return orderedRanks.Contains((int)Rank.Ace) && orderedRanks.Contains((int)Rank.King)
                    ? HandRank.RoyalFlush
                    : HandRank.StraightFlush;

            if (rankGroups[0].Count() == 4) return HandRank.FourOfAKind;
            if (rankGroups[0].Count() == 3 && rankGroups.Count > 1 && rankGroups[1].Count() >= 2)
                return HandRank.FullHouse;
            if (isFlush) return HandRank.Flush;
            if (isStraight) return HandRank.Straight;
            if (rankGroups[0].Count() == 3) return HandRank.ThreeOfAKind;
            if (rankGroups.Count(g => g.Count() == 2) >= 2) return HandRank.TwoPair;
            if (rankGroups.Count(g => g.Count() == 2) == 1) return HandRank.OnePair;

            return HandRank.HighCard;
        }


    }
}
