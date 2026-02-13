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
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

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
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public Guid CurrentTableId { get; internal set; } = Guid.Empty;

        public GamePhase Phase { get; private set; } = GamePhase.WaitingForPlayer;
        public int CurrentBet => _currentBet;
        public int CurrentPlayerIndex => _currentPlayerIndex;
        public IReadOnlyList<ICard> CommunityCards => _communityCards;
        public int MinBuyIn { get; private set; } = 200;
        public int MaxBuyIn { get; private set; } = 2000;
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
            try
            {
                CurrentTableId = tableId;
                Console.WriteLine($"[LOAD_DEBUG] Loading table {tableId}");

                var table = await _db.Tables
                    .Include(t => t.PlayerSeats)
                    .ThenInclude(ps => ps.Player)
                    .FirstOrDefaultAsync(t => t.Id == tableId);

                if (table == null)
                {
                    Console.WriteLine($"[LOAD_ERROR] Table {tableId} not found in DB");
                    throw new InvalidOperationException("Table not found");
                }

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

                // Store Buy-in limits
                MinBuyIn = table.MinBuyIn > 0 ? table.MinBuyIn : 200;
                MaxBuyIn = table.MaxBuyIn > 0 ? table.MaxBuyIn : 2000;

                // Debug: tampilkan seat yang tersedia
                Console.WriteLine($"[LOAD_DEBUG] Loaded table {tableId}: Seats={_seats.Count}, MinBuyIn={MinBuyIn}, MaxBuyIn={MaxBuyIn}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LOAD_ERROR] Failed to load table {tableId}: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
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
            try
            {
                Console.WriteLine($"[JOIN_DEBUG] Starting JoinTableAsync for {tableId}");
                // Load state table & seats
                await LoadPlayersFromTableAsync(tableId);

                var tableState = new TableStateDto
                {
                    TableId = tableId,
                    Phase = Phase,
                    CurrentBet = CurrentBet,
                    CommunityCards = CommunityCards
                                .Select(c => new Domain.Models.Card(c.Rank, c.Suit))
                                .ToList(),
                    // Masukkan seat state
                    Seats = GetSeatsState()
                };

                // Map each seat
                foreach (var seat in _seats.Where(s => s.IsOccupied).OrderBy(s => s.SeatIndex))
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

                Console.WriteLine($"[JOIN_DEBUG] Successfully joined table {tableId}");
                return ServiceResult<TableStateDto>.Success(tableState, "Table loaded");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[JOIN_ERROR] {ex.Message}\n{ex.StackTrace}");
                throw; // rethrow to keep 500 but also log to console
            }
        }
        // SitDown 

        public async Task<ServiceResult> SitDownAsync(Guid userId, string displayName, int seatIndex, int chips)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (chips <= 0) return ServiceResult.Fail("Chip amount must be positive");

                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                    var user = await _db.Users.FindAsync(userId);
                    if (user == null) return ServiceResult.Fail("User not found");
                    if (user.Balance < chips) return ServiceResult.Fail("Insufficient balance for buy-in");

                    // Global check: Is user already seated at ANY table?
                    var alreadySeated = await _db.Players.AnyAsync(p => p.UserId == userId && !p.isDeleted);
                    if (alreadySeated) return ServiceResult.Fail("You are already seated at a table. Stand up first to join another seat.");

                // Safety fallback if limits are 0 (stale memory or table)
                int effectiveMin = MinBuyIn > 0 ? MinBuyIn : 200;
                int effectiveMax = MaxBuyIn > 0 ? MaxBuyIn : 2000;

                if (chips < effectiveMin || chips > effectiveMax)
                {
                    var msg = $"Buy-in must be between {effectiveMin} and {effectiveMax}";
                    Console.WriteLine($"[SIT_FAIL] {msg}");
                    return ServiceResult.Fail(msg);
                }

                var seat = _seats.FirstOrDefault(s => s.SeatIndex == seatIndex);
                if (seat == null) return ServiceResult.Fail("Seat does not exist");
                if (seat.IsOccupied) return ServiceResult.Fail("Seat is already occupied");

                // Deduct from User Balance
                user.Balance -= chips;

                // Create Player entity
                var player = new Domain.Models.Player
                {
                    PlayerId = userId,
                    DisplayName = displayName,
                    ChipStack = chips,
                    CurrentBet = 0,
                    State = PlayerState.Active,
                    SeatIndex = seatIndex
                };

                seat.SitDown(player, chips);
                _players.Add(player);

                var dbPlayer = new Infrastructure.Persistence.Entities.Player
                {
                    UserId = userId,
                    DisplayName = displayName,
                    ChipStack = chips,
                    State = PlayerState.Active
                };
                _db.Players.Add(dbPlayer);

                var dbSeat = await _db.PlayerSeats.FirstOrDefaultAsync(ps => ps.SeatNumber == seatIndex && ps.TableId == CurrentTableId);
                if (dbSeat != null) 
                {
                    dbSeat.PlayerId = dbPlayer.Id; // Correctly map to the new Player record
                }
                else
                {
                    Console.WriteLine($"[SIT_FAIL] dbSeat not found for seat {seatIndex}");
                    return ServiceResult.Fail("Seat record not found in database");
                }

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                if (RoundStarted != null) await RoundStarted.Invoke();
                return ServiceResult.Success("Player sat down and buy-in successful");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"[SIT_FAIL] Failed to sit down: {ex.Message}");
                return ServiceResult.Fail($"Failed to sit down: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<ServiceResult> StandUpAsync(Guid userId)
        {
            await _semaphore.WaitAsync();
            try
            {
                using var transaction = await _db.Database.BeginTransactionAsync();
                try
                {
                var seat = _seats.FirstOrDefault(s => s.IsOccupied && s.Player!.PlayerId == userId);
                if (seat == null) return ServiceResult.Fail("Player is not seated");

                var player = seat.Player!;
                var chipsToReturn = player.ChipStack;

                var user = await _db.Users.FindAsync(userId);
                if (user != null) user.Balance += chipsToReturn;

                seat.Leave();
                _players.Remove(player);

                var dbPlayer = await _db.Players.FirstOrDefaultAsync(p => p.UserId == userId && !p.isDeleted);
                if (dbPlayer != null) _db.Players.Remove(dbPlayer);

                var dbSeat = await _db.PlayerSeats
                    .FirstOrDefaultAsync(ps => ps.PlayerId == userId && ps.TableId == CurrentTableId);
                if (dbSeat != null) dbSeat.PlayerId = null;

                await _db.SaveChangesAsync();
                await transaction.CommitAsync();

                if (RoundStarted != null) await RoundStarted.Invoke();
                return ServiceResult.Success($"Player stood up and {chipsToReturn} chips returned to balance");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"[STAND_FAIL] {ex.Message}");
                return ServiceResult.Fail($"Failed to stand up: {ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }
        // =========================
        // LEAVE TABLE
        // =========================
        public async Task<ServiceResult> LeaveTableAsync(Guid userId)
        {
            var isSeated = _players.Any(p => p.PlayerId == userId);
            if (isSeated)
            {
                return ServiceResult.Fail("You must stand up before leaving the table.");
            }

            return ServiceResult.Success("Player left table");
        }


        public List<IPlayer> ActivePlayers() => ActiveSeats().Select(s => s.Player!).ToList();
        // =========================
        // ROUND MANAGEMENT
        // =========================
        public async Task<ServiceResult> StartRound()
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

            await _db.SaveChangesAsync();

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
        public async Task<ServiceResult<int>> HandleBet(IPlayer player, int amount)
        {
            if (player.ChipStack < amount)
                return ServiceResult<int>.Fail("Not enough chips");

            player.ChipStack -= amount;
            player.CurrentBet += amount;

            if (player.CurrentBet > _currentBet)
                _currentBet = player.CurrentBet;

            await _db.SaveChangesAsync();
            return ServiceResult<int>.Success(_currentBet);
        }

        public async Task<ServiceResult<int>> HandleCall(IPlayer player) => await HandleBet(player, _currentBet - player.CurrentBet);

        public async Task<ServiceResult<int>> HandleRaise(IPlayer player, int raiseAmount)
        {
            var callResult = await HandleCall(player);
            if (!callResult.IsSuccess) return ServiceResult<int>.Fail(callResult.Message ?? "Call failed");
            return await HandleBet(player, raiseAmount);
        }

        public async Task<ServiceResult> HandleFold(IPlayer player)
        {
            player.State = PlayerState.Folded;
            await _db.SaveChangesAsync();
            return ServiceResult.Success("Folded");
        }

        public async Task<ServiceResult> HandleCheck(IPlayer player) => await Task.FromResult(ServiceResult.Success("Checked"));

        public async Task<ServiceResult> HandleAllIn(string playerName)
        {
            var player = _players.FirstOrDefault(p => p.DisplayName == playerName);
            if (player == null) return ServiceResult.Fail("Player not found");

            int amount = player.ChipStack;
            player.ChipStack = 0;
            player.CurrentBet += amount;
            player.State = PlayerState.AllIn;

            if (player.CurrentBet > _currentBet)
                _currentBet = player.CurrentBet;

            await _db.SaveChangesAsync();
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

        public async Task<List<IPlayer>> ResolveShowdown()
        {
            var winners = DetermineWinners();
            if (!winners.Any()) return winners;

            int pot = GetTotalPot();
            int share = pot / winners.Count;
            
            foreach (var winner in winners)
            {
                winner.ChipStack += share;
                winner.ChipsWonThisRound = share;
            }

            _lastShowdown = new ShowdownResult
            {
                Winners = winners,
                CommunityCards = _communityCards.ToList(),
                PlayerHands = _players.ToDictionary(p => p, p => p.Cards.ToList())
            };

            ShowdownCompleted?.Invoke();
            
            // Cleanup for next round
            _communityCards.Clear();
            foreach (var p in _players)
            {
                p.Cards.Clear();
                p.CurrentBet = 0;
                if (p.State == PlayerState.Folded || p.State == PlayerState.AllIn)
                    p.State = PlayerState.Active;
            }
            _currentBet = 0;
            _currentPlayerIndex = 0;
            await _db.SaveChangesAsync();

            return winners;
        }

        public async Task<(List<IPlayer> winners, HandRank rank)> ResolveShowdownDetailed()
        {
            var handResults = EvaluateHands();
            if (!handResults.Any())
                return (new List<IPlayer>(), HandRank.HighCard);

            HandRank bestRank = handResults.Values.Max();
            List<IPlayer> winners = handResults
                .Where(kv => kv.Value == bestRank)
                .Select(kv => kv.Key)
                .ToList();

            int pot = GetTotalPot();
            int share = pot / winners.Count;
            
            foreach (var winner in winners)
            {
                winner.ChipStack += share;
                winner.ChipsWonThisRound = share;
            }

            _lastShowdown = new ShowdownResult
            {
                Winners = winners,
                CommunityCards = _communityCards.ToList(),
                PlayerHands = _players.ToDictionary(p => p, p => p.Cards.ToList())
            };

            ShowdownCompleted?.Invoke();

            // Pot cleanup handled by engine logic usually, 
            // but here we ensure consistency
             _communityCards.Clear();
            foreach (var p in _players)
            {
                p.Cards.Clear();
                p.CurrentBet = 0;
                if (p.State == PlayerState.Folded || p.State == PlayerState.AllIn)
                    p.State = PlayerState.Active;
            }
            _currentBet = 0;
            _currentPlayerIndex = 0;
            Phase = GamePhase.PreFlop;
            await _db.SaveChangesAsync();

            return (winners, bestRank);
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
        private static HandRank EvaluateHand(List<ICard> cards)
        {
            if (cards.Count == 0) return HandRank.HighCard;

            var rankGroups = cards
                .GroupBy(c => c.Rank)
                .OrderByDescending(g => g.Count())
                .ThenByDescending(g => (int)g.Key)
                .ToList();

            var suitGroups = cards
                .GroupBy(c => c.Suit)
                .ToList();

            foreach (var suitGroup in suitGroups)
            {
                Rank? straightHigh = GetStraightHighCard(suitGroup.Select(c => c.Rank).ToList());
                if (straightHigh != null && suitGroup.Count() >= 5) return HandRank.StraightFlush;
            }

            if (rankGroups[0].Count() == 4) return HandRank.FourOfAKind;
            if (rankGroups[0].Count() == 3 && rankGroups.Any(g => g.Count() >= 2 && g != rankGroups[0])) return HandRank.FullHouse;
            if (suitGroups.Any(g => g.Count() >= 5)) return HandRank.Flush;
            if (GetStraightHighCard(cards.Select(c => c.Rank).ToList()) != null) return HandRank.Straight;
            if (rankGroups[0].Count() == 3) return HandRank.ThreeOfAKind;
            if (rankGroups.Count(g => g.Count() == 2) >= 2) return HandRank.TwoPair;
            if (rankGroups[0].Count() == 2) return HandRank.OnePair;

            return HandRank.HighCard;
        }

        private static Rank? GetStraightHighCard(List<Rank> ranks)
        {
            List<int> distinct = ranks.Select(r => (int)r).Distinct().OrderBy(r => r).ToList();

            if (distinct.Contains(14) && distinct.Take(4).Intersect(new[] { 2, 3, 4, 5 }).Count() == 4)
                return Rank.Five;

            for (int i = distinct.Count - 1; i >= 4; i--)
            {
                bool straight = true;
                for (int j = 0; j < 4; j++)
                {
                    if (distinct[i - j] != distinct[i - j - 1] + 1)
                    {
                        straight = false;
                        break;
                    }
                }
                if (straight) return (Rank)distinct[i];
            }

            return null;
        }

        public async Task<ServiceResult> ResetGame()
        {
            Phase = GamePhase.PreFlop;
            _currentBet = 0;
            _currentPlayerIndex = 0;
            _communityCards.Clear();
            _lastShowdown = null;

            foreach (var seat in _seats)
            {
                if (seat.IsOccupied)
                {
                    seat.Player!.CurrentBet = 0;
                    seat.Player.Cards.Clear();
                    seat.Player.State = PlayerState.Active;
                    seat.Player.ChipsWonThisRound = 0;
                }
            }

            await _db.SaveChangesAsync();
            return ServiceResult.Success("Game reset successful");
        }
    }
}
