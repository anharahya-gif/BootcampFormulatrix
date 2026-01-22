public class WildCard : Card
{
    public WildCard(CardType type)
        : base(null, type) { }

    public override bool CanPlayOn(Card topCard) => true;

    public void ChooseColor(string color)
    {
        Color = color;
    }
}
