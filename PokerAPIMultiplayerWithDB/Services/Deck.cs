using PokerAPIMultiplayerWithDB.Models;

namespace PokerAPIMultiplayerWithDB.Services
{
    public class Deck
    {
        private Stack<Card> _cards;

        public Deck()
        {
            _cards = new Stack<Card>(CreateDeck());
            Shuffle();
        }

        private List<Card> CreateDeck()
        {
            var deck = new List<Card>();

            foreach (Suit suit in Enum.GetValues(typeof(Suit)))
            {
                foreach (Rank rank in Enum.GetValues(typeof(Rank)))
                {
                    deck.Add(new Card { Rank = rank, Suit = suit });
                }
            }

            return deck;
        }

        public void Shuffle()
        {
            var rnd = Random.Shared;
            _cards = new Stack<Card>(_cards.OrderBy(c => rnd.Next()).ToList());
        }

        public Card DrawCard()
        {
            return _cards.Count > 0 ? _cards.Pop() : new Card { Rank = Rank.Ace, Suit = Suit.Clubs };
        }

        public int RemainingCards()
        {
            return _cards.Count;
        }
    }
}
