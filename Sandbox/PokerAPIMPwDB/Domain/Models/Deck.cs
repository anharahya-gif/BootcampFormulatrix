using PokerAPIMPwDB.Domain.Interfaces;
using PokerAPIMPwDB.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerAPIMPwDB.Domain.Models
{
    public class Deck : IDeck
    {
        private Stack<ICard> _cards;

        public Deck()
        {
            _cards = new Stack<ICard>();
            var suits = Enum.GetValues(typeof(Suit)).Cast<Suit>();
            var ranks = Enum.GetValues(typeof(Rank)).Cast<Rank>();

            foreach (var suit in suits)
            {
                foreach (var rank in ranks)
                {
                    _cards.Push(new Card(rank, suit));
                }
            }
        }

        public void Shuffle()
        {
            var rnd = new Random();
            _cards = new Stack<ICard>(_cards.OrderBy(c => rnd.Next()));
        }

        public ICard Draw()
        {
            return _cards.Pop();
        }

        public int RemainingCards()
        {
            return _cards.Count;
        }
    }
}
