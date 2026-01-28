using PokerAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerAPI.Services
{
    public class GameController
    {
        // ======================
        // Game State
        // ======================
        public Deck Deck { get; private set; } = new Deck();
        public Pot Pot { get; private set; } = new Pot();
        public Dictionary<Player, PlayerStatus> PlayerMap { get; private set; } = new Dictionary<Player, PlayerStatus>();
        public List<Card> CommunityCards { get; private set; } = new List<Card>();

        public int CurrentPlayerIndex { get; private set; } = 0;
        public int CurrentBet { get; private set; } = 0;
        public GamePhase Phase { get; private set; } = GamePhase.PreFlop;

        // ======================
        // Player Management
        // ======================
        public void AddPlayer(Player player)
        {
            if (!PlayerMap.ContainsKey(player))
                PlayerMap.Add(player, new PlayerStatus());
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
                status.CurrentBet = 0;
            CurrentBet = 0;

            // Set first active player
            CurrentPlayerIndex = PlayerMap.Keys.ToList().FindIndex(p => PlayerMap[p].State == PlayerState.Active);
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
            return active.All(p => PlayerMap[p].CurrentBet == CurrentBet || PlayerMap[p].State != PlayerState.Active);
        }

        // ======================
        // Betting Actions
        // ======================
        public bool HandleBet(Player player, int amount)
        {
            var status = PlayerMap[player];
            if (status.State != PlayerState.Active) return false;
            if (player.ChipStack < amount) return false;

            player.ChipStack -= amount;
            status.CurrentBet += amount;
            Pot.AddChips(amount);
            CurrentBet = Math.Max(CurrentBet, status.CurrentBet);

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
                status.State = PlayerState.AllIn;
            }
            else
            {
                player.ChipStack -= toCall;
                status.CurrentBet += toCall;
                Pot.AddChips(toCall);
            }

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
            Pot.AddChips(totalBet);
            CurrentBet = status.CurrentBet;

            return true;
        }

        public void HandleFold(Player player)
        {
            var status = PlayerMap[player];
            status.State = PlayerState.Folded;
        }

        public void HandleCheck(Player player)
        {
            var status = PlayerMap[player];
            if (CurrentBet == status.CurrentBet)
            {
                // Check allowed, nothing else needed
            }
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



    }
}
