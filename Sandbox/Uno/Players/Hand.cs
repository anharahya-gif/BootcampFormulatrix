using UnoGame.Cards;

namespace UnoGame.Players
{
    public class Hand
    {
        private readonly List<Card> _cards = new();

        public IReadOnlyList<Card> Cards => _cards;

        public void AddCard(Card card) => _cards.Add(card);
        public void RemoveCard(Card card) => _cards.Remove(card);
        public int Count() => _cards.Count;
        public bool IsEmpty() => _cards.Count == 0;
    }
}
