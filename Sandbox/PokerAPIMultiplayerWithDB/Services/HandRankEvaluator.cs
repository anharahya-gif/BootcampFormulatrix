using PokerAPIMultiplayerWithDB.Models;

namespace PokerAPIMultiplayerWithDB.Services
{
    public enum HandRank
    {
        HighCard = 1,
        Pair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush,
        RoyalFlush
    }

    public class EvaluatedHand
    {
        public HandRank Rank { get; set; }
        public List<int> Kickers { get; set; } = new();
    }

    public interface IHandRankEvaluator
    {
        EvaluatedHand EvaluateVisibleHand(IEnumerable<Card> playerCards, IEnumerable<Card> communityCards);
        // For showdown full evaluation
        EvaluatedHand EvaluateFinalHand(IEnumerable<Card> playerCards, IEnumerable<Card> communityCards);
    }

    public class HandRankEvaluator : IHandRankEvaluator
    {
        public EvaluatedHand EvaluateVisibleHand(IEnumerable<Card> playerCards, IEnumerable<Card> communityCards)
        {
            return Evaluate(playerCards.Concat(communityCards).ToList());
        }

        public EvaluatedHand EvaluateFinalHand(IEnumerable<Card> playerCards, IEnumerable<Card> communityCards)
        {
            return Evaluate(playerCards.Concat(communityCards).ToList());
        }

        private EvaluatedHand Evaluate(List<Card> cards)
        {
            if (cards == null || cards.Count < 1)
                return new EvaluatedHand { Rank = HandRank.HighCard, Kickers = new List<int>() };

            if (cards.Count < 5)
                return new EvaluatedHand { Rank = HandRank.HighCard, Kickers = cards.Select(c => (int)c.Rank).OrderByDescending(x => x).Take(5).ToList() };

            // Group by rank and suit
            var rankGroups = cards.GroupBy(c => c.Rank)
                                  .OrderByDescending(g => g.Count())
                                  .ThenByDescending(g => (int)g.Key)
                                  .ToList();

            var suitGroups = cards.GroupBy(c => c.Suit)
                                  .ToList();

            // Straight Flush / Royal Flush
            foreach (var suitGroup in suitGroups)
            {
                var straightHigh = GetStraightHighCard(suitGroup.Select(c => c.Rank).ToList());
                if (straightHigh != null)
                {
                    if (straightHigh == Rank.Ace)
                        return new EvaluatedHand { Rank = HandRank.RoyalFlush, Kickers = new List<int> { (int)straightHigh } };
                    return new EvaluatedHand { Rank = HandRank.StraightFlush, Kickers = new List<int> { (int)straightHigh } };
                }
            }

            // Four of a Kind
            if (rankGroups[0].Count() == 4)
            {
                var quadRank = (int)rankGroups[0].Key;
                var kicker = cards.Where(c => c.Rank != rankGroups[0].Key).Select(c => (int)c.Rank).OrderByDescending(x => x).FirstOrDefault();
                return new EvaluatedHand { Rank = HandRank.FourOfAKind, Kickers = new List<int> { quadRank, kicker } };
            }

            // Full House
            if (rankGroups[0].Count() == 3 && rankGroups.Any(g => g.Count() >= 2 && g != rankGroups[0]))
            {
                var three = (int)rankGroups[0].Key;
                var pair = (int)rankGroups.Where(g => g != rankGroups[0] && g.Count() >= 2).First().Key;
                return new EvaluatedHand { Rank = HandRank.FullHouse, Kickers = new List<int> { three, pair } };
            }

            // Flush
            var flushGroup = suitGroups.FirstOrDefault(g => g.Count() >= 5);
            if (flushGroup != null)
            {
                var topFive = flushGroup.Select(c => (int)c.Rank).OrderByDescending(x => x).Take(5).ToList();
                return new EvaluatedHand { Rank = HandRank.Flush, Kickers = topFive };
            }

            // Straight
            var straightHighCard = GetStraightHighCard(cards.Select(c => c.Rank).ToList());
            if (straightHighCard != null)
            {
                return new EvaluatedHand { Rank = HandRank.Straight, Kickers = new List<int> { (int)straightHighCard } };
            }

            // Three of a Kind
            if (rankGroups[0].Count() == 3)
            {
                var three = (int)rankGroups[0].Key;
                var kickers = cards.Where(c => c.Rank != rankGroups[0].Key).Select(c => (int)c.Rank).OrderByDescending(x => x).Take(2).ToList();
                var list = new List<int> { three };
                list.AddRange(kickers);
                return new EvaluatedHand { Rank = HandRank.ThreeOfAKind, Kickers = list };
            }

            // Two Pair
            if (rankGroups.Count(g => g.Count() == 2) >= 2)
            {
                var pairs = rankGroups.Where(g => g.Count() == 2).Take(2).Select(g => (int)g.Key).ToList();
                var kicker = cards.Where(c => !pairs.Contains((int)c.Rank)).Select(c => (int)c.Rank).OrderByDescending(x => x).FirstOrDefault();
                var list = new List<int>();
                list.AddRange(pairs);
                list.Add(kicker);
                return new EvaluatedHand { Rank = HandRank.TwoPair, Kickers = list };
            }

            // One Pair
            if (rankGroups[0].Count() == 2)
            {
                var pair = (int)rankGroups[0].Key;
                var kickers = cards.Where(c => c.Rank != rankGroups[0].Key).Select(c => (int)c.Rank).OrderByDescending(x => x).Take(3).ToList();
                var list = new List<int> { pair };
                list.AddRange(kickers);
                return new EvaluatedHand { Rank = HandRank.Pair, Kickers = list };
            }

            // High Card
            var highCards = cards.Select(c => (int)c.Rank).OrderByDescending(x => x).Take(5).ToList();
            return new EvaluatedHand { Rank = HandRank.HighCard, Kickers = highCards };
        }

        private static Rank? GetStraightHighCard(List<Rank> ranks)
        {
            var distinct = ranks.Select(r => (int)r).Distinct().OrderBy(r => r).ToList();

            // Low-Ace straight (A-2-3-4-5)
            if (distinct.Contains(14) && distinct.Take(4).SequenceEqual(new[] { 2, 3, 4, 5 }))
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
