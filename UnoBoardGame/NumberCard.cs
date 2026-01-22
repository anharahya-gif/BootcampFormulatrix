public class NumberCard : Card
{
    public int Number { get; }

    public NumberCard(string color, int number)
        : base(color, CardType.Number)
    {
        Number = number;
    }

    public override bool CanPlayOn(Card topCard)
    {
        if (topCard is NumberCard n)
            return n.Number == Number || n.Color == Color;

        return topCard.Color == Color;
    }

    public override string ToString()
    {
        return $"{Color} {Number}";
    }
}
