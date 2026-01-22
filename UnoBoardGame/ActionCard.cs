public class ActionCard : Card
{
    public ActionCard(string color, CardType type)
        : base(color, type) { }

    public override bool CanPlayOn(Card topCard)
    {
        return topCard.Color == Color || topCard.Type == Type;
    }
}
