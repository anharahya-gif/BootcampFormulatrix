
using System.Collections.Generic;

namespace UnoBoardGame.Models;

public class Deck
{
    private Stack<Card> _cards = new();

    public int Count => _cards.Count;

    public void Add(Card card)
    {
        _cards.Push(card);
    }

    public Card Draw()
    {
        return _cards.Pop();
    }

    public void RefillFrom(IEnumerable<Card> cards)
    {
        foreach (var c in cards)
            _cards.Push(c);
    }

    public void Shuffle()
    {
        var list = _cards.ToList();
        _cards.Clear();

        var rnd = new Random();
        foreach (var c in list.OrderBy(_ => rnd.Next()))
            _cards.Push(c);
    }

}
public class DiscardPile
{
    private Stack<Card> _cards = new();

    public int Count => _cards.Count;

    public void Add(Card card)
    {
        _cards.Push(card);
    }

    public Card Top()
    {
        return _cards.Peek();
    }

    public Card PopTop()
    {
        return _cards.Pop();
    }

    public List<Card> PopAll()
    {
        var list = _cards.ToList();
        _cards.Clear();
        return list;
    }
}
