namespace UnoBoardGame.Models
{
    public class Player
    {
        public string Name { get; set; }
        public bool IsHuman { get; set; }
        public Dictionary<CardType, List<Card>> Cards { get; set; }
        public bool HasCalledUno { get; set; }

        public Player(string name, bool isHuman)
        {
            Name = name;
            IsHuman = isHuman;
            HasCalledUno = false;

            Cards = new Dictionary<CardType, List<Card>>();
            foreach (CardType type in Enum.GetValues(typeof(CardType)))
                Cards[type] = new List<Card>();
        }

        public List<Card> GetAllCards()
            => Cards.Values.SelectMany(c => c).ToList();
    }
}