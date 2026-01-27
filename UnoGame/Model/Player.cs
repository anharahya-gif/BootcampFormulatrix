using System.Collections.Generic;

namespace UnoGame.Model
{
    public abstract class Player
    {
        public string Name { get; }
        public Dictionary<CardType, List<Card>> Cards { get; } = new();
        public bool HasCalledUno { get; set; }

        protected Player(string name)
        {
            Name = name;

            foreach (CardType type in Enum.GetValues<CardType>())
                Cards[type] = new List<Card>();
        }

        public int CardCount => Cards.Sum(c => c.Value.Count);

        public override string ToString() => Name;
    }
    public class HumanPlayer : Player
    {
        public HumanPlayer(string name) : base(name) { }
    }
    public class BotPlayer : Player
    {
        public BotPlayer(string name) : base(name) { }
    }

}
