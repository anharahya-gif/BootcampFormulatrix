public abstract class Card
{
    public string Color { get; protected set; }
    public CardType Type { get; }

    protected Card(string color, CardType type)
    {
        Color = color;
        Type = type;
    }

    public abstract bool CanPlayOn(Card topCard);

    public override string ToString()
    {
        return Color == null ? Type.ToString() : $"{Color} {Type}";
    }
}


public enum CardType
{
    Number,
    Skip,
    Reverse,
    DrawTwo,
    Wild,
    WildDrawFour
}

