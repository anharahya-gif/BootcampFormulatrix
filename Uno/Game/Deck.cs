using System;
using System.Collections.Generic;
using UnoGame.Cards;

namespace UnoGame.Game
{
    public class Deck
    {
        private Stack<Card> _cards = new();

        public Deck()
        {
            InitializeUnoDeck();
            Shuffle();
        }

        private void InitializeUnoDeck()
        {
            var temp = new List<Card>();

            foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
            {
                // Number cards
                temp.Add(new NumberCard(color, 0)); // one zero
                for (int i = 1; i <= 9; i++)
                {
                    temp.Add(new NumberCard(color, i));
                    temp.Add(new NumberCard(color, i));
                }

                // Action cards (2 each per color)
                temp.Add(new SkipCard(color));
                temp.Add(new SkipCard(color));

                temp.Add(new ReverseCard(color));
                temp.Add(new ReverseCard(color));

                temp.Add(new DrawTwoCard(color));
                temp.Add(new DrawTwoCard(color));
            }

            // Wild cards
            for (int i = 0; i < 4; i++)
            {
                temp.Add(new WildCard());
                temp.Add(new WildDrawFourCard());
            }

            foreach (var card in temp)
                _cards.Push(card);
        }

        public void Shuffle()
        {
            var rnd = new Random();
            var list = new List<Card>(_cards);
            _cards.Clear();

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rnd.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }

            foreach (var card in list)
                _cards.Push(card);
        }

        public Card Draw()
        {
            if (_cards.Count == 0)
                throw new InvalidOperationException("Deck is empty");

            return _cards.Pop();
        }
        public void AddCards(List<Card> cards)
        {
            foreach (var card in cards)
                _cards.Push(card);
        }

        public bool IsEmpty() => _cards.Count == 0;

        public int Count => _cards.Count;
    }
}
