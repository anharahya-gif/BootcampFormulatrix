public abstract class Player
{
    public string Name { get; }
    public List<Card> Hand { get; } = new();

    public bool HasCalledUno { get; protected set; }

    protected Player(string name)
    {
        Name = name;
    }

    public void DrawCard(Deck deck)
    {
        Hand.Add(deck.Draw());
        HasCalledUno = false; // reset
    }

    public abstract Card TakeTurn(Card topCard, Deck deck);

    public virtual void CallUno()
    {
        HasCalledUno = true;
        Console.WriteLine($"{Name} says UNO!");
    }
}
