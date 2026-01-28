namespace PokerAPI.Models
{
    public enum Suit
    {
        Hearts,
        Diamonds,
        Clubs,
        Spades
    }

    public enum Rank
    {
        Two = 2,
        Three = 3,
        Four = 4,
        Five = 5,
        Six = 6,
        Seven = 7,
        Eight = 8,
        Nine = 9,
        Ten = 10,
        Jack = 11,
        Queen = 12,
        King = 13,
        Ace = 14
    }

    public enum HandRank
    {
        HighCard = 1,
        Pair,
        TwoPair,
        ThreeOfAKind,
        Straight,
        Flush,
        FullHouse,
        FourOfAKind,
        StraightFlush
    }

    public enum GamePhase
    {
        PreFlop,
        Flop,
        Turn,
        River,
        Showdown
    }

    public enum PlayerState
    {
        Active,
        Folded,
        AllIn,
        Eliminated
    }
}
