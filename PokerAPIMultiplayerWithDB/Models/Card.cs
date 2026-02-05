namespace PokerAPIMultiplayerWithDB.Models
{
    public enum Suit { Clubs = 1, Diamonds = 2, Hearts = 3, Spades = 4 }
    public enum Rank
    {
        Two = 2, Three = 3, Four = 4, Five = 5, Six = 6, Seven = 7,
        Eight = 8, Nine = 9, Ten = 10, Jack = 11, Queen = 12, King = 13, Ace = 14
    }

    public class Card
    {
        public Rank Rank { get; set; }
        public Suit Suit { get; set; }

        public Card() { }
        public Card(Rank rank, Suit suit)
        {
            Rank = rank; Suit = suit;
        }
    }
}
