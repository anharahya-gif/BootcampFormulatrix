namespace PokerFrontend.Models;

public class AuthResponse
{
    public string Token { get; set; } = "";
    public int ExpiresInMinutes { get; set; }
}

public class Table
{
    public int Id { get; set; }
    public string TableName { get; set; } = "";
    public string Status { get; set; } = "";
    public long MinBuyIn { get; set; }
    public long MaxBuyIn { get; set; }
    public int MaxPlayers { get; set; }
    public int CurrentPlayers { get; set; }
}

public class GameState
{
    public int TableId { get; set; }
    public string Phase { get; set; } = "";
    public List<Card> CommunityCards { get; set; } = new();
    public long Pot { get; set; }
    public long CurrentBet { get; set; }
    public int DealerSeatNumber { get; set; }
    public int SmallBlindSeatNumber { get; set; }
    public int BigBlindSeatNumber { get; set; }
    public int CurrentPlayerSeatNumber { get; set; }
    public bool IsGameActive { get; set; }
    public int RoundNumber { get; set; }
    public List<PlayerInfo> Players { get; set; } = new();
}

public class PlayerInfo
{
    public int SeatNumber { get; set; }
    public string Username { get; set; } = "";
    public long ChipStack { get; set; }
    public bool HasFolded { get; set; }
    public bool IsAllIn { get; set; }
    public long CurrentBet { get; set; }
    public bool HasActed { get; set; }
    public List<Card> HoleCards { get; set; } = new();
}

public class Card
{
    public string Rank { get; set; } = "";
    public string Suit { get; set; } = "";

    public override string ToString() => $"{Rank}{Suit[0]}";
}

public class PlayerActionRequest
{
    public string Action { get; set; } = "";
    public long? Amount { get; set; }
}
