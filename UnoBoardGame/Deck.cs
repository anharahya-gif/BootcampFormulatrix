public class Deck
{
    private Stack<Card> drawPile;
    private Stack<Card> discardPile = new();
    private static Random rnd = new();

    public Deck()
    {
        var cards = new List<Card>();
        string[] colors = { "Red", "Blue", "Green", "Yellow" };

        foreach (var c in colors)
        {
            // Number cards (0–9) x2 kecuali 0
            cards.Add(new NumberCard(c, 0));
            for (int i = 1; i <= 9; i++)
            {
                cards.Add(new NumberCard(c, i));
                cards.Add(new NumberCard(c, i));
            }

            // Action cards (2 per warna)
            cards.Add(new ActionCard(c, CardType.Skip));
            cards.Add(new ActionCard(c, CardType.Skip));

            cards.Add(new ActionCard(c, CardType.Reverse));
            cards.Add(new ActionCard(c, CardType.Reverse));

            cards.Add(new ActionCard(c, CardType.DrawTwo));
            cards.Add(new ActionCard(c, CardType.DrawTwo));
        }

        // Wild cards
        for (int i = 0; i < 4; i++)
        {
            cards.Add(new WildCard(CardType.Wild));
            cards.Add(new WildCard(CardType.WildDrawFour));
        }

        Shuffle(cards);
        drawPile = new Stack<Card>(cards);
    }

    public Card Draw()
    {
        if (drawPile.Count == 0)
            Reshuffle();

        return drawPile.Pop();
    }

    public void Discard(Card card)
    {
        discardPile.Push(card);
    }

    private void Reshuffle()
    {
        if (discardPile.Count <= 1)
            throw new InvalidOperationException("Tidak cukup kartu untuk reshuffle!");

        Card top = discardPile.Pop();
        var temp = new List<Card>(discardPile);
        discardPile.Clear();

        Shuffle(temp);
        drawPile = new Stack<Card>(temp);
        discardPile.Push(top);
    }

    private void Shuffle(List<Card> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
