using UnoGame.Cards;

namespace UnoGame.Players
{
    public class Player
    {
        public string Name { get; }
        public Hand Hand { get; } = new();
        public bool HasCalledUno { get; private set; }

        public Player(string name)
        {
            Name = name;
        }

        public void PlayCard(Card card)
        {
            Hand.RemoveCard(card);
            HasCalledUno = false;
        }

        public void DrawCard(Game.Deck deck)
        {
            Hand.AddCard(deck.Draw());
        }

        public void CallUno()
        {
            HasCalledUno = true;
        }
    }
}
