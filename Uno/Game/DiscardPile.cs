using System.Collections.Generic;
using UnoGame.Cards;

namespace UnoGame.Game
{
    public class DiscardPile
    {
        private readonly Stack<Card> _cards = new();

        public void Add(Card card)
        {
            _cards.Push(card);
        }

        public Card Top()
        {
            return _cards.Peek();
        }

        public List<Card> TakeAllExceptTop()
        {
            var top = _cards.Pop();
            var rest = new List<Card>(_cards);
            _cards.Clear();
            _cards.Push(top);
            return rest;
        }
        public bool IsEmpty() => _cards.Count == 0;

    }
}
