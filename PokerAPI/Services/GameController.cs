
using PokerAPI.Models;
using PokerAPI.Models.DTOs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using PokerAPI.Services.Interfaces;
using System.Linq;

namespace PokerAPI.Services
{
    public class GameController : IGameController, IDisposable
    {
        // ======================
        // Game State / Core Properties
        // ======================
        private bool _hasRoundStarted = false;
        private const int MaxPlayers = 10;

        public IDeck Deck { get; private set; } = new Deck();
        public IPot Pot { get; private set; } = new Pot();
        public Dictionary<IPlayer, PlayerStatus> PlayerMap { get; private set; } = new Dictionary<IPlayer, PlayerStatus>();
        public List<ICard> CommunityCards { get; private set; } = new List<ICard>();

        public int CurrentPlayerIndex { get; private set; } = 0;
        public int CurrentBet { get; private set; } = 0;
        public GamePhase Phase { get; private set; } = GamePhase.PreFlop;
        public ShowdownResult? LastShowdown { get; private set; }

        // ======================
        // Events
        // ======================
        public event Action? RoundStarted;
        public event Action? CommunityCardsUpdated;
        public event Action? ShowdownCompleted;

        // ======================
        // Constructor
        // ======================
        public GameController()
        {
            RoundStarted += OnRoundStarted;
            CommunityCardsUpdated += OnCommunityCardsUpdated;
            ShowdownCompleted += OnShowdownCompleted;
        }

        // ======================
        // Event Handlers
        // ======================
        private void OnRoundStarted()
        {
            Console.WriteLine("[Event] RoundStarted triggered");
        }

        private void OnCommunityCardsUpdated()
        {
            Console.WriteLine("[Event] CommunityCardsUpdated triggered. CommunityCards: " +
                string.Join(", ", CommunityCards.Select(c => $"{c.Rank} of {c.Suit}")));
        }

        private void OnShowdownCompleted()
        {
            Console.WriteLine("[Event] ShowdownCompleted triggered");
            if (LastShowdown != null)
            {
                Console.WriteLine("Winners: " + string.Join(", ", LastShowdown.Winners.Select(p => p.Name)));
                Console.WriteLine("Winning Rank: " + LastShowdown.HandRank);
            }
        }

        // ======================
        // Dispose / Event Cleanup
        // ======================
        public void UnsubscribeEvents()
        {
            RoundStarted -= OnRoundStarted;
            CommunityCardsUpdated -= OnCommunityCardsUpdated;
            ShowdownCompleted -= OnShowdownCompleted;
        }

        public void Dispose()
        {
            UnsubscribeEvents();
            GC.SuppressFinalize(this);
        }

        // ======================
        // Public State Accessors
        // ======================
        public IEnumerable<PlayerPublicState> GetPlayersPublicState()
        {
            return PlayerMap.Select(kv =>
            {
                var player = kv.Key;
                var status = kv.Value;

                return new PlayerPublicState
                {
                    SeatIndex = player.SeatIndex,
                    Name = player.Name,
                    ChipStack = player.ChipStack,
                    State = status.State.ToString(),
                    CurrentBet = status.CurrentBet,
                    IsFolded = status.State == PlayerState.Folded,
                    Hand = status.Hand.Select(c => $"{c.Rank} of {c.Suit}").ToList()
                };
            });
        }

        public object? EvaluateVisibleForPlayer(string playerName)
        {
            var kv = PlayerMap.FirstOrDefault(k => k.Key.Name == playerName);
            if (kv.Key == null) return null;

            var player = kv.Key;
            var status = kv.Value;

            var combined = status.Hand.Concat(CommunityCards).ToList();
            var rank = EvaluateHand(combined);

            return new
            {
                Player = player.Name,
                SeatIndex = player.SeatIndex,
                Hand = status.Hand.Select(c => $"{c.Rank} of {c.Suit}").ToList(),
                CommunityCards = CommunityCards.Select(c => $"{c.Rank} of {c.Suit}").ToList(),
                Rank = rank.ToString()
            };
        }

        public object GetShowdownDetails()
        {
            var details = PlayerMap.Select(kv =>
            {
                var player = kv.Key;
                var status = kv.Value;
                var combined = status.Hand.Concat(CommunityCards).ToList();
                var rank = EvaluateHand(combined);
                return new
                {
                    Player = player.Name,
                    SeatIndex = player.SeatIndex,
                    Hand = status.Hand.Select(c => $"{c.Rank} of {c.Suit}").ToList(),
                    Rank = rank.ToString()
                };
            }).ToList();

            return new
            {
                CommunityCards = CommunityCards.Select(c => $"{c.Rank} of {c.Suit}").ToList(),
                Players = details,
                Winners = DetermineWinners().Select(p => p.Name).ToList()
            };
        }

        public int GetTotalPot() => Pot.TotalChips;
        public IPlayer? GetPlayerByName(string name) => PlayerMap.Keys.FirstOrDefault(p => p.Name == name);
        public int GetTotalPlayers() => PlayerMap.Count;

        public string GetGameState()
        {
            if (PlayerMap.Count < 2) return "WaitingForPlayers";
            if (Phase == GamePhase.Showdown) return "Completed";
            return _hasRoundStarted ? "InProgress" : "WaitingForStartRound";
        }

        public bool CanStartRound()
        {
            if (PlayerMap.Count < 2) return false;
            if (!_hasRoundStarted) return true;
            return Phase == GamePhase.Showdown;
        }

        // ======================
        // Player Management
        // ======================
        public void AddPlayer(string name, int chips, int seatIndex)
        {
            if (PlayerMap.Count >= MaxPlayers)
                throw new InvalidOperationException($"Table is full (max {MaxPlayers} players)");

            if (PlayerMap.Keys.Any(p => p.Name == name))
                throw new InvalidOperationException("Player already exists");

            var player = new Player(name, chips) { SeatIndex = seatIndex };
            var occupiedSeats = PlayerMap.Keys.Select(p => p.SeatIndex).ToList();

            if (seatIndex < 0 || seatIndex >= MaxPlayers)
                throw new InvalidOperationException("Seat index invalid");

            if (occupiedSeats.Contains(seatIndex))
                throw new InvalidOperationException("Seat already occupied");

            PlayerMap[player] = new PlayerStatus();
        }

        public void RemovePlayer(IPlayer player)
        {
            if (!PlayerMap.ContainsKey(player)) return;

            int removedIndex = PlayerMap.Keys.ToList().IndexOf(player);
            PlayerMap.Remove(player);

            if (PlayerMap.Count == 0)
            {
                CurrentPlayerIndex = 0;
                _hasRoundStarted = false;
                Phase = GamePhase.PreFlop;
            }
            else if (removedIndex <= CurrentPlayerIndex)
            {
                CurrentPlayerIndex--;
                if (CurrentPlayerIndex < 0) CurrentPlayerIndex = 0;
            }
        }

        public List<IPlayer> ActivePlayers()
        {
            return PlayerMap.Where(kv => kv.Value.State == PlayerState.Active || kv.Value.State == PlayerState.AllIn)
                            .Select(kv => kv.Key).ToList();
        }

        // ======================
        // Round Management
        // ======================
        public void StartRound()
        {
            if (!CanStartRound())
                throw new InvalidOperationException("Cannot start round in current state because of insufficient players.");
            else if (_hasRoundStarted && Phase != GamePhase.Showdown)
                throw new InvalidOperationException("Round already in progress.");

            _hasRoundStarted = true;
            Deck = new Deck();
            Deck.Shuffle();
            Pot.Reset();
            CurrentBet = 0;
            Phase = GamePhase.PreFlop;
            CommunityCards.Clear();
            CurrentPlayerIndex = 0;

            foreach (var status in PlayerMap.Values)
                status.ResetStatus();

            DealHoleCards();
            Debug.WriteLine("Round started");
            RoundStarted?.Invoke();
        }

        private void DealHoleCards()
        {
            foreach (var player in PlayerMap.Keys)
            {
                PlayerMap[player].Hand.Clear();
                PlayerMap[player].Hand.Add(Deck.Draw());
                PlayerMap[player].Hand.Add(Deck.Draw());
            }
        }

        private void DealFlop()
        {
            if (Deck.RemainingCards() >= 3)
            {
                CommunityCards.Add(Deck.Draw());
                CommunityCards.Add(Deck.Draw());
                CommunityCards.Add(Deck.Draw());
                CommunityCardsUpdated?.Invoke();
            }
        }

        private void DealTurn()
        {
            if (Deck.RemainingCards() >= 1)
            {
                CommunityCards.Add(Deck.Draw());
                CommunityCardsUpdated?.Invoke();
            }
        }

        private void DealRiver()
        {
            if (Deck.RemainingCards() >= 1)
            {
                CommunityCards.Add(Deck.Draw());
                CommunityCardsUpdated?.Invoke();
            }
        }

        public void NextPhase()
        {
            switch (Phase)
            {
                case GamePhase.PreFlop:
                    DealFlop();
                    StartBettingRound();
                    Phase = GamePhase.Flop;
                    break;
                case GamePhase.Flop:
                    DealTurn();
                    StartBettingRound();
                    Phase = GamePhase.Turn;
                    break;
                case GamePhase.Turn:
                    DealRiver();
                    StartBettingRound();
                    Phase = GamePhase.River;
                    break;
                case GamePhase.River:
                    Phase = GamePhase.Showdown;
                    break;
            }
        }

        private void StartBettingRound()
        {
            foreach (var status in PlayerMap.Values)
            {
                status.CurrentBet = 0;
                status.HasActed = false;
            }
            CurrentBet = 0;

            CurrentPlayerIndex = Math.Max(0,
            PlayerMap.Keys.ToList()
            .FindIndex(p => PlayerMap[p].State == PlayerState.Active));
        }

        // ======================
        // Player Turn Management
        // ======================
        public IPlayer? GetCurrentPlayer()
        {
            var activePlayers = ActivePlayers();
            if (!activePlayers.Any()) return null;

            var playerList = PlayerMap.Keys.ToList();
            if (CurrentPlayerIndex < 0 || CurrentPlayerIndex >= playerList.Count)
                CurrentPlayerIndex = 0;

            var currentPlayer = playerList[CurrentPlayerIndex];

            if (PlayerMap[currentPlayer].State != PlayerState.Active)
                return GetNextActivePlayer();

            return currentPlayer;
        }

        public IPlayer? GetNextActivePlayer()
        {
            var playerList = PlayerMap.Keys.ToList();
            int count = playerList.Count;
            if (count == 0) return null;

            for (int i = 1; i <= count; i++)
            {
                int nextIndex = (CurrentPlayerIndex + i) % count;
                var nextPlayer = playerList[nextIndex];
                if (PlayerMap[nextPlayer].State == PlayerState.Active)
                {
                    CurrentPlayerIndex = nextIndex;
                    return nextPlayer;
                }
            }

            return null;
        }

        public bool IsBettingRoundOver()
        {
            var active = ActivePlayers();
            return active.All(p => PlayerMap[p].HasActed && PlayerMap[p].CurrentBet == CurrentBet);
        }

        // ======================
        // Betting Actions
        // ======================
        public bool HandleBet(IPlayer player, int amount)
        {
            if (amount <= 0)
                throw new InvalidOperationException("Bet amount must be greater than 0");

            if (Phase == GamePhase.Showdown)
                throw new InvalidOperationException("Cannot bet at showdown");

            var status = PlayerMap[player];

            if (status.State != PlayerState.Active)
                throw new InvalidOperationException("Player cannot bet in current state");

            if (player.ChipStack < amount)
                throw new InvalidOperationException("Insufficient chips");

            player.ChipStack -= amount;
            status.CurrentBet += amount;
            status.HasActed = true;
            Pot.AddChips(amount);
            CurrentBet = Math.Max(CurrentBet, status.CurrentBet);
            TryAutoAdvance();
            return true;
        }

        public bool HandleCall(IPlayer player)
        {
            var status = PlayerMap[player];
            if (status.State != PlayerState.Active) return false;

            int toCall = CurrentBet - status.CurrentBet;
            if (player.ChipStack <= toCall)
            {
                Pot.AddChips(player.ChipStack);
                status.CurrentBet += player.ChipStack;
                player.ChipStack = 0;
                status.HasActed = true;
                status.State = PlayerState.AllIn;
            }
            else
            {
                player.ChipStack -= toCall;
                status.CurrentBet += toCall;
                Pot.AddChips(toCall);
                status.HasActed = true;
            }
            TryAutoAdvance();
            return true;
        }

        public bool HandleRaise(IPlayer player, int raiseAmount)
        {
            var status = PlayerMap[player];
            if (status.State != PlayerState.Active) return false;

            int totalBet = (CurrentBet - status.CurrentBet) + raiseAmount;
            if (player.ChipStack < totalBet) return false;

            player.ChipStack -= totalBet;
            status.CurrentBet += totalBet;
            status.HasActed = true;
            Pot.AddChips(totalBet);
            CurrentBet = status.CurrentBet;
            TryAutoAdvance();
            return true;
        }

        public void HandleFold(IPlayer player)
        {
            var status = PlayerMap[player];
            if (status.State != PlayerState.Active) return;

            bool wasCurrent = GetCurrentPlayer() == player;

            status.State = PlayerState.Folded;
            status.HasActed = true;
            status.Hand.Clear();

            if (wasCurrent)
                GetNextActivePlayer();

            TryAutoAdvance();
        }

        public void HandleCheck(IPlayer player)
        {
            var status = PlayerMap[player];
            if (CurrentBet == status.CurrentBet)
                status.HasActed = true;

            TryAutoAdvance();
        }

        public bool HandleAllIn(string playerName)
        {
            var player = PlayerMap.Keys.FirstOrDefault(p => p.Name == playerName);
            if (player == null) return false;

            var status = PlayerMap[player];
            if (status.State != PlayerState.Active) return false;

            int amount = player.ChipStack;
            if (amount <= 0) return false;

            player.ChipStack = 0;
            status.CurrentBet += amount;
            status.State = PlayerState.AllIn;
            status.HasActed = true;

            Pot.AddChips(amount);
            CurrentBet = Math.Max(CurrentBet, status.CurrentBet);
            TryAutoAdvance();
            return true;
        }

        // ======================
        // Showdown
        // ======================
        public Dictionary<IPlayer, HandRank> EvaluateHands()
        {
            var result = new Dictionary<IPlayer, HandRank>();
            foreach (var kv in PlayerMap)
            {
                var player = kv.Key;
                var status = kv.Value;
                if (status.State == PlayerState.Folded) continue;

                var combinedCards = status.Hand.Concat(CommunityCards).ToList();
                result[player] = EvaluateHand(combinedCards);
            }
            return result;
        }

        public List<IPlayer> DetermineWinners()
        {
            var hands = EvaluateHands();
            if (!hands.Any()) return new List<IPlayer>();

            var maxRank = hands.Values.Max();
            var winners = hands.Where(kv => kv.Value == maxRank).Select(kv => kv.Key).ToList();
            return winners;
        }

        public List<IPlayer> ResolveShowdown()
        {
            var winners = DetermineWinners();
            if (!winners.Any()) return winners;

            int share = Pot.TotalChips / winners.Count;
            foreach (var winner in winners)
                winner.ChipStack += share;

            Pot.Reset();
            return winners;
        }

        public (List<IPlayer> winners, HandRank rank) ResolveShowdownDetailed()
        {
            if (Phase != GamePhase.Showdown)
                return (new List<IPlayer>(), HandRank.HighCard);

            var handResults = EvaluateHands();
            if (!handResults.Any())
                return (new List<IPlayer>(), HandRank.HighCard);

            var bestRank = handResults.Values.Max();
            var winners = handResults
                .Where(kv => kv.Value == bestRank)
                .Select(kv => kv.Key)
                .ToList();

            int share = Pot.TotalChips / winners.Count;
            foreach (var winner in winners)
                winner.ChipStack += share;

            Pot.Reset();
            LastShowdown = new ShowdownResult(winners, bestRank);
            CleanupAfterRound();

            _hasRoundStarted = false;
            Phase = GamePhase.PreFlop;

            ShowdownCompleted?.Invoke();

            return (winners, bestRank);
        }

        private void CleanupAfterRound()
        {
            CommunityCards.Clear();

            foreach (var status in PlayerMap.Values)
            {
                status.Hand.Clear();
                status.CurrentBet = 0;
                status.HasActed = false;

                if (status.State != PlayerState.Active)
                    status.State = PlayerState.Active;
            }

            CurrentBet = 0;
            CurrentPlayerIndex = 0;
        }

        // ======================
        // Internal Helpers
        // ======================
        private bool NoMoreActionsPossible()
        {
            var alive = PlayerMap.Values
                .Where(s => s.State != PlayerState.Folded)
                .ToList();

            if (alive.All(s => s.State == PlayerState.AllIn))
                return true;

            var active = alive.Where(s => s.State == PlayerState.Active).ToList();
            return !active.Any();
        }

        private void TryAutoAdvance()
        {
            var active = ActivePlayers();

            if (active.Count == 1)
            {
                Phase = GamePhase.Showdown;
                ResolveShowdownDetailed();
                return;
            }

            if (NoMoreActionsPossible())
            {
                DealRemainingCommunityCards();
                Phase = GamePhase.Showdown;
                ResolveShowdownDetailed();
                return;
            }

            if (IsBettingRoundOver())
            {
                NextPhase();
                if (Phase == GamePhase.Showdown)
                    ResolveShowdownDetailed();
            }
        }

        private void DealRemainingCommunityCards()
        {
            if (Phase == GamePhase.PreFlop)
            {
                DealFlop();
                DealTurn();
                DealRiver();
            }
            else if (Phase == GamePhase.Flop)
            {
                DealTurn();
                DealRiver();
            }
            else if (Phase == GamePhase.Turn)
            {
                DealRiver();
            }
        }

        // ======================
        // Internal Hand Evaluator
        // ======================
        private static HandRank EvaluateHand(List<ICard> cards)
        {
            if (cards.Count < 5)
                return HandRank.HighCard;

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
                var straightHigh = GetStraightHighCard(
                    suitGroup.Select(c => c.Rank).ToList()
                );

                if (straightHigh != null)
                    return HandRank.StraightFlush;
            }

            if (rankGroups[0].Count() == 4) return HandRank.FourOfAKind;
            if (rankGroups[0].Count() == 3 && rankGroups.Any(g => g.Count() >= 2 && g != rankGroups[0])) return HandRank.FullHouse;
            if (suitGroups.Any(g => g.Count() >= 5)) return HandRank.Flush;
            if (GetStraightHighCard(cards.Select(c => c.Rank).ToList()) != null) return HandRank.Straight;
            if (rankGroups[0].Count() == 3) return HandRank.ThreeOfAKind;
            if (rankGroups.Count(g => g.Count() == 2) >= 2) return HandRank.TwoPair;
            if (rankGroups[0].Count() == 2) return HandRank.Pair;

            return HandRank.HighCard;
        }

        private static Rank? GetStraightHighCard(List<Rank> ranks)
        {
            var distinct = ranks
                .Select(r => (int)r)
                .Distinct()
                .OrderBy(r => r)
                .ToList();

            if (distinct.Contains(14) &&
                distinct.Take(4).SequenceEqual(new[] { 2, 3, 4, 5 }))
                return Rank.Five;

            for (int i = 0; i <= distinct.Count - 5; i++)
            {
                bool straight = true;
                for (int j = 0; j < 4; j++)
                {
                    if (distinct[i + j + 1] != distinct[i + j] + 1)
                    {
                        straight = false;
                        break;
                    }
                }

                if (straight)
                    return (Rank)distinct[i + 4];
            }

            return null;
        }
    }
}
