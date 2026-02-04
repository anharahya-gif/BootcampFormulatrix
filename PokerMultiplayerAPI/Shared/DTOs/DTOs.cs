using PokerMultiplayerAPI.Domain.Enums;

namespace PokerMultiplayerAPI.Shared.DTOs;

public class CardDto
{
    public string Suit { get; set; }
    public string Rank { get; set; }
    
    // Helper to map from Entity
    public static CardDto FromEntity(PokerMultiplayerAPI.Domain.Entities.Card card)
    {
        return new CardDto { Suit = card.Suit.ToString(), Rank = card.Rank.ToString() };
    }
}

public class PlayerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Chips { get; set; }
    public decimal CurrentBet { get; set; }
    public bool HasFolded { get; set; }
    public bool IsAllIn { get; set; }
    public bool IsMyTurn { get; set; }
    public int SeatIndex { get; set; }
    // Only populated for the requesting player or during showdown
    public List<CardDto> HoleCards { get; set; } = new(); 
}

public class TableStateDto
{
    public Guid TableId { get; set; }
    public string TableName { get; set; }
    public string Phase { get; set; }
    public decimal Pot { get; set; }
    public List<CardDto> CommunityCards { get; set; } = new();
    public List<PlayerDto> Players { get; set; } = new();
    public decimal CurrentMaxBet { get; set; }
}

public class JoinTableRequest
{
    public string PlayerName { get; set; }
    public decimal BuyIn { get; set; }
}

public class PlayerActionRequest
{
    public TurnAction Action { get; set; }
    public decimal Amount { get; set; } // For Raise
}
