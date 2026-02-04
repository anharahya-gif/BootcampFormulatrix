namespace PokerMultiplayerAPI.Domain.Enums;

public enum Suit
{
    Hearts,
    Diamonds,
    Clubs,
    Spades
}

public enum Rank
{
    Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten,
    Jack = 11, Queen = 12, King = 13, Ace = 14
}

public enum GamePhase
{
    WaitingForPlayers,
    PreFlop,
    Flop,
    Turn,
    River,
    Showdown
}

public enum TurnAction
{
    Fold,
    Check,
    Call,
    Raise,
    AllIn
}

public enum HandRank
{
    HighCard,
    Pair,
    TwoPair,
    ThreeOfAKind,
    Straight,
    Flush,
    FullHouse,
    FourOfAKind,
    StraightFlush,
    RoyalFlush
}
