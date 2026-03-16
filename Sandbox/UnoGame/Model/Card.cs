namespace UnoGame.Model
{
    public class Card
    {
        public CardType Type { get; }
        public CardColor? Color { get; }
        public int? Number { get; }

        public Card(CardType type, CardColor? color = null, int? number = null)
        {
            Type = type;
            Color = color;
            Number = number;
        }

        public override string ToString()
        {
            if (Type == CardType.Number)
                return $"{Color} {Number}";

            if (Color.HasValue)
                return $"{Color} {Type}";

            return Type.ToString();
        }
    }
}
