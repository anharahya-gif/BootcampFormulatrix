using System;   
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;       
namespace UnoBoardGame.Models; 
public class Deck
{
    public Stack<Card> Cards { get; private set; } = new();

    public void RefillFrom(IEnumerable<Card> cards)
    {
        Cards = new Stack<Card>(cards);
    }

    public void Shuffle()
    {
        var rnd = new Random();
        Cards = new Stack<Card>(Cards.OrderBy(_ => rnd.Next()));
    }

    public Card Draw() => Cards.Pop();
}


public class DiscardPile
{
    public Stack<Card> Cards { get; private set; } = new();

    public Card Top() => Cards.Peek();
}
