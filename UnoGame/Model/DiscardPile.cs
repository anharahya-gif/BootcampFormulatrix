using System.Collections.Generic;

namespace UnoGame.Model
{
    public class DiscardPile
    {
        internal Stack<Card> Cards { get; }

        public DiscardPile()
        {
            Cards = new Stack<Card>();
        }
    }
}
