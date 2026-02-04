
using PokerAPI.Services.Interfaces;

namespace PokerAPI.Models
{
    public class Deck : IDeck
    {
        private Stack<Card> _cards;

        public Deck()
        {
            _cards = new Stack<Card>(CreateDeck());
        }

        private List<Card> CreateDeck()
        {
            var deck = new List<Card>();

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    deck.Add(new Card(rank, suit));
                }
            }

            return deck;
        }

        public void Shuffle()
        {
            var rnd = Random.Shared;
            _cards = new Stack<Card>(_cards.OrderBy(c => rnd.Next()));
        }

        public Card Draw()
        {
            return _cards.Count > 0 ? _cards.Pop() : null;
        }

        public int RemainingCards()
        {
            return _cards.Count;
        }
    }
}
