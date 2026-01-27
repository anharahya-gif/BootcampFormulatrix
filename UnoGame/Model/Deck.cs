using System.Collections.Generic;

namespace UnoGame.Model
{
    public class Deck
    {
        internal Stack<Card> Cards { get; }

        public Deck()
        {
            Cards = new Stack<Card>();
        }
    }
}
