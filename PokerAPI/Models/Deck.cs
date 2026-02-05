
using PokerAPI.Services.Interfaces;

namespace PokerAPI.Models
{
    public class Deck : IDeck
    {
        private Stack<ICard> _cards;

        public Deck()
        {
            _cards = new Stack<ICard>(CreateDeck());
        }

        private List<ICard> CreateDeck()
        {
            var deck = new List<ICard>();

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
            _cards = new Stack<ICard>(_cards.OrderBy(c => rnd.Next()));
        }

        public ICard Draw()
        {
            return _cards.Count > 0 ? _cards.Pop() : null;
        }

        public int RemainingCards()
        {
            return _cards.Count;
        }
    }
}
