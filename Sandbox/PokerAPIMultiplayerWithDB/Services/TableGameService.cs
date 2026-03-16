using PokerAPIMultiplayerWithDB.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerAPIMultiplayerWithDB.Services
{
    // Enum for game phases matching PokerAPI
    public enum GamePhase
    {
        PreFlop,
        Flop,
        Turn,
        River,
        Showdown
    }

    // Player state at table during game
    public class TablePlayerState
    {
        public int PlayerId { get; set; }
        public string Username { get; set; } = "";
        public int SeatNumber { get; set; }
        public long ChipStack { get; set; } // Current chips in hand
        public List<Card> HoleCards { get; set; } = new();
        public bool HasFolded { get; set; }
        public bool IsAllIn { get; set; }
        public long CurrentBet { get; set; }
        public bool HasActed { get; set; }
    }

    // Game state for a table
    public class TableGameState
    {
        public int TableId { get; set; }
        public GamePhase Phase { get; set; } = GamePhase.PreFlop;
        public List<Card> CommunityCards { get; set; } = new();
        public Deck Deck { get; set; } = null!;
        public long Pot { get; set; }
        public long CurrentBet { get; set; } // Highest bet in current betting round
        public long SmallBlindAmount { get; set; }
        public long BigBlindAmount { get; set; }
        public int DealerSeatNumber { get; set; } // Seat number (1-10) of dealer
        public int SmallBlindSeatNumber { get; set; }
        public int BigBlindSeatNumber { get; set; }
        public int CurrentPlayerSeatNumber { get; set; } // Whose turn
        public bool IsGameActive { get; set; }
        public Dictionary<int, TablePlayerState> Players { get; set; } = new(); // Key: SeatNumber
        public List<int> ActionOrder { get; set; } = new(); // Seat numbers in action order
        public int RoundNumber { get; set; }
        public DateTime StartedAt { get; set; }
    }

    public interface ITableGameService
    {
        TableGameState GetGameState(int tableId);
        bool StartGame(int tableId, Dictionary<int, (int playerId, string username, long chipDeposit)> seatToPlayerMap, long? smallBlind = null, long? bigBlind = null);
        bool PlayerAction(int tableId, int playerId, string action, long? amount);
        void EndHand(int tableId);
        void MoveToNextPhase(int tableId);
        event Action<int, TableGameState>? GameStateUpdated; // tableId, newState
        event Action<int, string>? GameEventOccurred; // tableId, eventMessage
    }

    public class TableGameService : ITableGameService
    {
        private readonly Dictionary<int, TableGameState> _tableGames = new();
        private readonly IHandRankEvaluator _handRankEvaluator;

        public event Action<int, TableGameState>? GameStateUpdated;
        public event Action<int, string>? GameEventOccurred;

        // Default blind amounts (can be overridden per table)
        private const long DefaultSmallBlind = 10;
        private const long DefaultBigBlind = 20;

        public TableGameService(IHandRankEvaluator handRankEvaluator)
        {
            _handRankEvaluator = handRankEvaluator;
        }

        public TableGameState GetGameState(int tableId)
        {
            if (_tableGames.TryGetValue(tableId, out var state))
            {
                return state;
            }
            return new TableGameState { TableId = tableId };
        }

        public bool StartGame(int tableId, Dictionary<int, (int playerId, string username, long chipDeposit)> seatToPlayerMap, long? smallBlind = null, long? bigBlind = null)
        {
            // Validate minimum players
            var activePlayers = seatToPlayerMap.Where(kv => kv.Value.chipDeposit > 0).ToList();
            if (activePlayers.Count < 2)
            {
                GameEventOccurred?.Invoke(tableId, "Need at least 2 players to start game");
                return false;
            }

            var state = new TableGameState
            {
                TableId = tableId,
                Phase = GamePhase.PreFlop,
                IsGameActive = true,
                RoundNumber = 1,
                StartedAt = DateTime.UtcNow,
                Pot = 0,
                CurrentBet = 0,
                CommunityCards = new(),
                DealerSeatNumber = 0, // Will be set below
                SmallBlindAmount = smallBlind ?? DefaultSmallBlind,
                BigBlindAmount = bigBlind ?? DefaultBigBlind
            };

            // Initialize players
            var seatNumbers = seatToPlayerMap.Keys.OrderBy(s => s).ToList();
            foreach (var seatNum in seatNumbers)
            {
                var (playerId, username, chipDeposit) = seatToPlayerMap[seatNum];
                if (chipDeposit > 0)
                {
                    state.Players[seatNum] = new TablePlayerState
                    {
                        PlayerId = playerId,
                        Username = username,
                        SeatNumber = seatNum,
                        ChipStack = chipDeposit,
                        HoleCards = new(),
                        HasFolded = false,
                        IsAllIn = false,
                        CurrentBet = 0,
                        HasActed = false
                    };
                }
            }

            // Set dealer (lowest seat number for first hand)
            state.DealerSeatNumber = state.Players.Keys.Min();
            state.SmallBlindSeatNumber = GetNextActiveSeat(state, state.DealerSeatNumber);
            state.BigBlindSeatNumber = GetNextActiveSeat(state, state.SmallBlindSeatNumber);
            state.CurrentPlayerSeatNumber = GetNextActiveSeat(state, state.BigBlindSeatNumber); // First to act

            // Build action order
            state.ActionOrder = GetActionOrder(state, state.BigBlindSeatNumber).ToList();

            // Deal hole cards
            state.Deck = new Deck();
            state.Deck.Shuffle();
            foreach (var seat in state.Players.Values)
            {
                seat.HoleCards = new() { state.Deck.DrawCard(), state.Deck.DrawCard() };
            }

            // Post blinds using configured amounts
            var sbAmt = state.SmallBlindAmount;
            var bbAmt = state.BigBlindAmount;
            if (state.Players.TryGetValue(state.SmallBlindSeatNumber, out var sbPlayer) && sbPlayer.ChipStack > 0)
            {
                var posted = Math.Min(sbAmt, sbPlayer.ChipStack);
                sbPlayer.CurrentBet = posted;
                sbPlayer.ChipStack -= posted;
                state.Pot += posted;
            }

            if (state.Players.TryGetValue(state.BigBlindSeatNumber, out var bbPlayer) && bbPlayer.ChipStack > 0)
            {
                var posted = Math.Min(bbAmt, bbPlayer.ChipStack);
                bbPlayer.CurrentBet = posted;
                bbPlayer.ChipStack -= posted;
                state.Pot += posted;
            }

            state.CurrentBet = bbAmt;

            _tableGames[tableId] = state;
            GameStateUpdated?.Invoke(tableId, state);
            GameEventOccurred?.Invoke(tableId, $"Game started! Dealer: Seat {state.DealerSeatNumber}, SB: {state.SmallBlindAmount}, BB: {state.BigBlindAmount}");
            return true;
        }

        public bool PlayerAction(int tableId, int playerId, string action, long? amount)
        {
            if (!_tableGames.TryGetValue(tableId, out var state))
                return false;

            // Find player
            var playerEntry = state.Players.FirstOrDefault(kv => kv.Value.PlayerId == playerId);
            if (playerEntry.Value == null)
            {
                GameEventOccurred?.Invoke(tableId, $"Player {playerId} not found at table");
                return false;
            }

            var player = playerEntry.Value;
            int seatNum = playerEntry.Key;

            // Validate it's this player's turn
            if (state.CurrentPlayerSeatNumber != seatNum)
            {
                GameEventOccurred?.Invoke(tableId, $"Not your turn. Current player: Seat {state.CurrentPlayerSeatNumber}");
                return false;
            }

            // Validate player hasn't already acted this round
            if (player.HasActed && action.ToLower() != "allin")
            {
                GameEventOccurred?.Invoke(tableId, "You have already acted this round");
                return false;
            }

            bool actionValid = false;
            string eventMsg = "";

            switch (action.ToLower())
            {
                case "fold":
                    player.HasFolded = true;
                    eventMsg = $"Player {player.Username} (Seat {seatNum}) folded";
                    actionValid = true;
                    break;

                case "check":
                    if (state.CurrentBet > player.CurrentBet)
                    {
                        GameEventOccurred?.Invoke(tableId, "Cannot check with outstanding bet");
                        return false;
                    }
                    eventMsg = $"Player {player.Username} (Seat {seatNum}) checked";
                    actionValid = true;
                    break;

                case "call":
                    long callAmount = Math.Min(state.CurrentBet - player.CurrentBet, player.ChipStack);
                    player.ChipStack -= callAmount;
                    player.CurrentBet += callAmount;
                    state.Pot += callAmount;
                    if (callAmount == 0)
                        eventMsg = $"Player {player.Username} (Seat {seatNum}) called (no chips to call)";
                    else
                        eventMsg = $"Player {player.Username} (Seat {seatNum}) called {callAmount}";
                    actionValid = true;
                    break;

                case "bet":
                case "raise":
                    if (amount == null || amount <= 0)
                    {
                        GameEventOccurred?.Invoke(tableId, "Invalid bet/raise amount");
                        return false;
                    }
                    // Enforce minimum raise: at least big blind
                    var minRaise = Math.Max(state.BigBlindAmount, state.CurrentBet - player.CurrentBet + state.BigBlindAmount);
                    var requested = amount.Value;
                    if (action.ToLower() == "raise" && requested < minRaise)
                    {
                        GameEventOccurred?.Invoke(tableId, $"Raise must be at least {minRaise}");
                        return false;
                    }

                    long betAmount = Math.Min(requested, player.ChipStack);
                    if (betAmount <= 0)
                    {
                        GameEventOccurred?.Invoke(tableId, "Insufficient chips");
                        return false;
                    }

                    player.ChipStack -= betAmount;
                    player.CurrentBet += betAmount;
                    state.Pot += betAmount;
                    state.CurrentBet = Math.Max(state.CurrentBet, player.CurrentBet);

                    eventMsg = $"Player {player.Username} (Seat {seatNum}) {action} {betAmount}";
                    actionValid = true;
                    break;

                case "allin":
                    long allInAmount = player.ChipStack;
                    if (allInAmount > 0)
                    {
                        player.IsAllIn = true;
                        player.ChipStack = 0;
                        player.CurrentBet += allInAmount;
                        state.Pot += allInAmount;
                        if (state.CurrentBet < player.CurrentBet)
                            state.CurrentBet = player.CurrentBet;
                        eventMsg = $"Player {player.Username} (Seat {seatNum}) went all-in for {allInAmount}";
                    }
                    else
                    {
                        eventMsg = $"Player {player.Username} (Seat {seatNum}) all-in (no chips)";
                    }
                    actionValid = true;
                    break;

                default:
                    GameEventOccurred?.Invoke(tableId, $"Unknown action: {action}");
                    return false;
            }

            if (!actionValid)
                return false;

            player.HasActed = true;
            GameEventOccurred?.Invoke(tableId, eventMsg);

            // Check if betting round is complete
            var activePlayers = state.Players.Values.Where(p => !p.HasFolded).ToList();
            var unactedActivePlayers = activePlayers.Where(p => !p.HasActed && !p.IsAllIn).ToList();

            if (unactedActivePlayers.Count == 0)
            {
                // Betting round complete
                BettingRoundComplete(tableId);
            }
            else
            {
                // Move to next player
                MoveToNextPlayer(state);
            }

            GameStateUpdated?.Invoke(tableId, state);
            return true;
        }

        public void MoveToNextPhase(int tableId)
        {
            if (!_tableGames.TryGetValue(tableId, out var state))
                return;

            switch (state.Phase)
            {
                case GamePhase.PreFlop:
                    state.Phase = GamePhase.Flop;
                    state.CommunityCards = new() { DealCard(state), DealCard(state), DealCard(state) };
                    GameEventOccurred?.Invoke(tableId, $"Flop: {string.Join(" ", state.CommunityCards)}");
                    break;

                case GamePhase.Flop:
                    state.Phase = GamePhase.Turn;
                    state.CommunityCards.Add(DealCard(state));
                    GameEventOccurred?.Invoke(tableId, $"Turn: {state.CommunityCards.Last()}");
                    break;

                case GamePhase.Turn:
                    state.Phase = GamePhase.River;
                    state.CommunityCards.Add(DealCard(state));
                    GameEventOccurred?.Invoke(tableId, $"River: {state.CommunityCards.Last()}");
                    break;

                case GamePhase.River:
                    state.Phase = GamePhase.Showdown;
                    DetermineWinners(tableId);
                    return;
            }

            // Reset for new betting round
            foreach (var player in state.Players.Values)
            {
                player.CurrentBet = 0;
                player.HasActed = false;
            }

            state.CurrentBet = 0;
            state.CurrentPlayerSeatNumber = GetNextActiveSeat(state, state.DealerSeatNumber);

            GameStateUpdated?.Invoke(tableId, state);
        }

        public void EndHand(int tableId)
        {
            if (!_tableGames.TryGetValue(tableId, out var state))
                return;

            // Rotate dealer button
            state.DealerSeatNumber = GetNextActiveSeat(state, state.DealerSeatNumber);
            state.SmallBlindSeatNumber = GetNextActiveSeat(state, state.DealerSeatNumber);
            state.BigBlindSeatNumber = GetNextActiveSeat(state, state.SmallBlindSeatNumber);

            // Reset player states
            foreach (var player in state.Players.Values)
            {
                player.HoleCards.Clear();
                player.HasFolded = false;
                player.IsAllIn = false;
                player.CurrentBet = 0;
                player.HasActed = false;
            }

            state.Phase = GamePhase.PreFlop;
            state.CommunityCards.Clear();
            state.Pot = 0;
            state.CurrentBet = 0;
            state.RoundNumber++;

            GameStateUpdated?.Invoke(tableId, state);
        }

        // ==================== PRIVATE HELPERS ====================

        private void BettingRoundComplete(int tableId)
        {
            if (!_tableGames.TryGetValue(tableId, out var state))
                return;

            var activePlayers = state.Players.Values.Where(p => !p.HasFolded).ToList();

            if (activePlayers.Count == 1)
            {
                // Only one player left, they win
                var winner = activePlayers.First();
                GameEventOccurred?.Invoke(tableId, $"Player {winner.Username} wins (others folded)!");
                DetermineWinners(tableId);
            }
            else if (state.Phase == GamePhase.River)
            {
                // River complete, go to showdown
                state.Phase = GamePhase.Showdown;
                DetermineWinners(tableId);
            }
            else
            {
                // Move to next phase
                MoveToNextPhase(tableId);
            }
        }

        private void DetermineWinners(int tableId)
        {
            if (!_tableGames.TryGetValue(tableId, out var state))
                return;

            var activePlayers = state.Players.Values.Where(p => !p.HasFolded).ToList();

            if (activePlayers.Count == 0)
            {
                GameEventOccurred?.Invoke(tableId, "No players remaining");
                return;
            }

            if (activePlayers.Count == 1)
            {
                var winner = activePlayers.First();
                winner.ChipStack += state.Pot;
                GameEventOccurred?.Invoke(tableId, $"Player {winner.Username} (Seat {winner.SeatNumber}) wins {state.Pot}!");
                EndHand(tableId);
                return;
            }

            // Evaluate hands at showdown
            var handRankings = new List<(TablePlayerState player, int rank, List<int> kickers)>();

            foreach (var player in activePlayers)
            {
                var allCards = new List<Card>(player.HoleCards);
                allCards.AddRange(state.CommunityCards);

                var evaluated = _handRankEvaluator.EvaluateFinalHand(player.HoleCards, state.CommunityCards);
                handRankings.Add((player, (int)evaluated.Rank, evaluated.Kickers));
            }

            // Sort by rank (descending) then kickers (descending)
            var winners = handRankings
                .OrderByDescending(h => h.rank)
                .ThenByDescending(h => string.Join(",", h.kickers))
                .ToList();

            var bestRank = winners[0].rank;
            var bestKickers = winners[0].kickers;
            var winnerList = winners.Where(h => h.rank == bestRank && h.kickers.SequenceEqual(bestKickers)).ToList();

            if (winnerList.Count == 1)
            {
                var winner = winnerList[0].player;
                winner.ChipStack += state.Pot;
                GameEventOccurred?.Invoke(tableId, $"Player {winner.Username} (Seat {winner.SeatNumber}) wins {state.Pot} with hand rank {bestRank}!");
            }
            else
            {
                // Split pot
                var splitAmount = state.Pot / winnerList.Count;
                foreach (var (player, _, _) in winnerList)
                {
                    player.ChipStack += splitAmount;
                }
                var winners_str = string.Join(", ", winnerList.Select(w => $"{w.player.Username} (Seat {w.player.SeatNumber})"));
                GameEventOccurred?.Invoke(tableId, $"Players {winners_str} split pot of {state.Pot}!");
            }

            EndHand(tableId);
        }

        private void MoveToNextPlayer(TableGameState state)
        {
            var activePlayers = state.Players.Values.Where(p => !p.HasFolded && !p.IsAllIn).ToList();
            if (activePlayers.Count == 0)
                return;

            state.CurrentPlayerSeatNumber = GetNextActiveSeat(state, state.CurrentPlayerSeatNumber);
        }

        private int GetNextActiveSeat(TableGameState state, int currentSeat)
        {
            var activePlayers = state.Players.Keys.OrderBy(s => s).ToList();
            var currentIndex = activePlayers.IndexOf(currentSeat);
            if (currentIndex < 0)
                currentIndex = 0;

            return activePlayers[(currentIndex + 1) % activePlayers.Count];
        }

        private IEnumerable<int> GetActionOrder(TableGameState state, int startSeat)
        {
            var seats = state.Players.Keys.OrderBy(s => s).ToList();
            var startIndex = seats.IndexOf(startSeat);
            if (startIndex < 0)
                startIndex = 0;

            for (int i = 0; i < seats.Count; i++)
            {
                yield return seats[(startIndex + i) % seats.Count];
            }
        }

        private Card DealCard(TableGameState state)
        {
            if (state == null || state.Deck == null)
            {
                // Fallback to a fresh deck (shouldn't happen)
                var d = new Deck();
                d.Shuffle();
                return d.DrawCard();
            }

            return state.Deck.DrawCard();
        }
    }
}
