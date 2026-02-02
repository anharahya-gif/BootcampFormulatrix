using PokerAPI.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PokerAPI.Services
{
    public class GameController
    {
        // ======================
        // Game State
        // ======================
        private bool _hasRoundStarted = false;
        private const int MaxPlayers = 10;
        public Deck Deck { get; private set; } = new Deck();
        public Pot Pot { get; private set; } = new Pot();
        public Dictionary<Player, PlayerStatus> PlayerMap { get; private set; } = new Dictionary<Player, PlayerStatus>();
        public List<Card> CommunityCards { get; private set; } = new List<Card>();

        public int CurrentPlayerIndex { get; private set; } = 0;
        public int CurrentBet { get; private set; } = 0;
        public GamePhase Phase { get; private set; } = GamePhase.PreFlop;
        public event Action? RoundStarted;

        public ShowdownResult? LastShowdown { get; private set; }

        public string GetGameState()
        {
            if (PlayerMap.Count < 2)
                return "WaitingForPlayers";
            else if (Phase == GamePhase.Showdown)
                return "Completed";
            else if (_hasRoundStarted)
                return "InProgress";
            else
                return "WaitingForStartRound";
        }


        public bool CanStartRound()
        {
            if (PlayerMap.Count < 2)
                return false;

            // Belum pernah mulai
            if (!_hasRoundStarted)
                return true;

            // Sudah selesai (showdown)
            return Phase == GamePhase.Showdown;
        }

        // ======================
        // Player Management
        // ======================
        public void AddPlayer(Player player)
        {
            if (PlayerMap.Count >= MaxPlayers)
                throw new InvalidOperationException("Table is full (max 10 players)");

            if (PlayerMap.ContainsKey(player))
                throw new InvalidOperationException("Player already exists");

            // ============================
            // Tentukan seat index secara random
            // ============================
            var occupiedSeats = PlayerMap.Keys.Select(p => p.SeatIndex).ToList();
            var availableSeats = Enumerable.Range(0, MaxPlayers)
                                           .Where(i => !occupiedSeats.Contains(i))
                                           .ToList();

            if (!availableSeats.Any())
                throw new InvalidOperationException("No seats available");

            var rnd = new Random();
            player.SeatIndex = availableSeats[rnd.Next(availableSeats.Count)];

            // ============================
            // Tambahkan player ke map
            // ============================
            PlayerMap[player] = new PlayerStatus();
        }


        public void RemovePlayer(Player player)
        {
            if (PlayerMap.ContainsKey(player))
                PlayerMap.Remove(player);
        }

        public List<Player> ActivePlayers()
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
            }
        }

        private void DealTurn()
        {
            if (Deck.RemainingCards() >= 1)
                CommunityCards.Add(Deck.Draw());
        }

        private void DealRiver()
        {
            if (Deck.RemainingCards() >= 1)
                CommunityCards.Add(Deck.Draw());
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

            // Set first active player
            CurrentPlayerIndex = Math.Max(0,
            PlayerMap.Keys.ToList()
            .FindIndex(p => PlayerMap[p].State == PlayerState.Active));

        }

        // ======================
        // Player Turn Management
        // ======================
        public Player GetCurrentPlayer()
        {
            var activePlayers = ActivePlayers();
            if (!activePlayers.Any()) return null;

            var playerList = PlayerMap.Keys.ToList();
            var player = playerList[CurrentPlayerIndex];
            if (PlayerMap[player].State != PlayerState.Active)
                return GetNextActivePlayer();

            return player;
        }

        public Player GetNextActivePlayer()
        {
            var playerList = PlayerMap.Keys.ToList();
            int count = playerList.Count;
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
            return null; // semua pemain fold/all-in
        }

        public bool IsBettingRoundOver()
        {
            var active = ActivePlayers();

            return active.All(p =>
                PlayerMap[p].HasActed &&
                PlayerMap[p].CurrentBet == CurrentBet
            );
        }

        // ======================
        // Betting Actions
        // ======================
        public bool HandleBet(Player player, int amount)
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

        public bool HandleCall(Player player)
        {
            var status = PlayerMap[player];
            if (status.State != PlayerState.Active) return false;

            int toCall = CurrentBet - status.CurrentBet;
            if (player.ChipStack <= toCall)
            {
                // All-in
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

        public bool HandleRaise(Player player, int raiseAmount)
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

        public void HandleFold(Player player)
        {
            var status = PlayerMap[player];
            if (status.State != PlayerState.Active)
                return;

            bool wasCurrent = GetCurrentPlayer() == player;

            status.State = PlayerState.Folded;
            status.HasActed = true;
            status.Hand.Clear();

            if (wasCurrent)
                GetNextActivePlayer();

            TryAutoAdvance();
        }


        public void HandleCheck(Player player)
        {
            var status = PlayerMap[player];
            if (CurrentBet == status.CurrentBet)
            {
                status.HasActed = true;
            }
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
        public Dictionary<Player, HandRank> EvaluateHands()
        {
            var result = new Dictionary<Player, HandRank>();
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

        public List<Player> DetermineWinners()
        {
            var hands = EvaluateHands();
            if (!hands.Any()) return new List<Player>();

            var maxRank = hands.Values.Max();
            var winners = hands.Where(kv => kv.Value == maxRank).Select(kv => kv.Key).ToList();
            return winners;
        }

        // ======================
        // Internal Hand Evaluator
        // ======================
        private static HandRank EvaluateHand(List<Card> cards)
        {
            if (cards.Count < 5)
                return HandRank.HighCard;

            // ======================
            // Grouping
            // ======================
            var rankGroups = cards
                .GroupBy(c => c.Rank)
                .OrderByDescending(g => g.Count())
                .ThenByDescending(g => (int)g.Key)
                .ToList();

            var suitGroups = cards
                .GroupBy(c => c.Suit)
                .ToList();

            // ======================
            // Straight Flush
            // ======================
            foreach (var suitGroup in suitGroups)
            {
                var straightHigh = GetStraightHighCard(
                    suitGroup.Select(c => c.Rank).ToList()
                );

                if (straightHigh != null)
                    return HandRank.StraightFlush;
            }

            // ======================
            // Four of a Kind
            // ======================
            if (rankGroups[0].Count() == 4)
                return HandRank.FourOfAKind;

            // ======================
            // Full House
            // ======================
            if (rankGroups[0].Count() == 3 && rankGroups.Any(g => g.Count() >= 2 && g != rankGroups[0]))
                return HandRank.FullHouse;

            // ======================
            // Flush
            // ======================
            if (suitGroups.Any(g => g.Count() >= 5))
                return HandRank.Flush;

            // ======================
            // Straight
            // ======================
            if (GetStraightHighCard(cards.Select(c => c.Rank).ToList()) != null)
                return HandRank.Straight;

            // ======================
            // Three of a Kind
            // ======================
            if (rankGroups[0].Count() == 3)
                return HandRank.ThreeOfAKind;

            // ======================
            // Two Pair
            // ======================
            if (rankGroups.Count(g => g.Count() == 2) >= 2)
                return HandRank.TwoPair;

            // ======================
            // One Pair
            // ======================
            if (rankGroups[0].Count() == 2)
                return HandRank.Pair;

            return HandRank.HighCard;
        }
        private static Rank? GetStraightHighCard(List<Rank> ranks)
        {
            var distinct = ranks
                .Select(r => (int)r)
                .Distinct()
                .OrderBy(r => r)
                .ToList();

            // Low-Ace straight (A-2-3-4-5)
            if (distinct.Contains(14) &&
                distinct.Take(4).SequenceEqual(new[] { 2, 3, 4, 5 }))
            {
                return Rank.Five;
            }

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
        public List<Player> ResolveShowdown()
        {
            var winners = DetermineWinners();
            if (!winners.Any()) return winners;

            int share = Pot.TotalChips / winners.Count;

            foreach (var winner in winners)
            {
                winner.ChipStack += share;
            }

            Pot.Reset();
            return winners;
        }
        public (List<Player> winners, HandRank rank) ResolveShowdownDetailed()
        {
            if (Phase != GamePhase.Showdown)
                return (new List<Player>(), HandRank.HighCard);

            var handResults = EvaluateHands();
            if (!handResults.Any())
                return (new List<Player>(), HandRank.HighCard);

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

            return (winners, bestRank);
        }

        private void CleanupAfterRound()
        {
            // Clear community cards
            CommunityCards.Clear();

            // Clear player hands & reset status
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

        private bool NoMoreActionsPossible()
        {
            var alive = PlayerMap.Values
                .Where(s => s.State != PlayerState.Folded)
                .ToList();

            // Semua all-in
            if (alive.All(s => s.State == PlayerState.AllIn))
                return true;

            // Tidak ada Active yang bisa raise
            var active = alive.Where(s => s.State == PlayerState.Active).ToList();
            if (!active.Any())
                return true;

            return false;
        }

        private void TryAutoAdvance()
        {
            // 1️⃣ Tinggal 1 player → langsung menang
            if (ActivePlayers().Count == 1)
            {
                Phase = GamePhase.Showdown;
                ResolveShowdownDetailed();
                return;
            }

            // 2️⃣ Semua all-in → buka sisa kartu → showdown
            if (NoMoreActionsPossible())
            {
                DealRemainingCommunityCards();
                Phase = GamePhase.Showdown;
                ResolveShowdownDetailed();
                return;
            }

            // 3️⃣ Betting normal selesai → lanjut phase
            if (IsBettingRoundOver())
            {
                NextPhase();
                if (Phase == GamePhase.Showdown)
                {
                    ResolveShowdownDetailed();
                }
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








    }
}