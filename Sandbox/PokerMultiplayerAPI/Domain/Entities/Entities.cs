using PokerMultiplayerAPI.Domain.Enums;

namespace PokerMultiplayerAPI.Domain.Entities;

public class Card
{
    public Suit Suit { get; set; }
    public Rank Rank { get; set; }
    
    public Card(Suit suit, Rank rank)
    {
        Suit = suit;
        Rank = rank;
    }
    
    public override string ToString() => $"{Rank} of {Suit}";
}

public class Player
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty; // SignalR ConnectionId mapping
    public decimal Chips { get; set; }
    public decimal CurrentBet { get; set; } // Bet in current round
    public bool HasFolded { get; set; }
    public bool IsAllIn { get; set; }
    public bool IsActive { get; set; } = true; // For disconnect handling
    public List<Card> HoleCards { get; set; } = new();

    public void ResetForNewRound()
    {
        CurrentBet = 0;
        HasFolded = false;
        IsAllIn = false;
        HoleCards.Clear();
    }
}

public class Pot
{
    public decimal TotalAmount { get; set; }
    // Could handle side-pots here later
}

public class GameState
{
    public Guid TableId { get; set; }
    public GamePhase Phase { get; set; } = GamePhase.WaitingForPlayers;
    public decimal CurrentPot { get; set; }
    public List<Card> CommunityCards { get; set; } = new();
    public Guid CurrentTurnPlayerId { get; set; }
    public decimal CurrentMaxBet { get; set; } // To validate Call/Raise
    public DateTime TurnStartTime { get; set; } // For timer
    
    // Track who has acted in the current betting round
    // This is cleared when the Phase changes OR when someone Raises (which re-opens betting)
    public HashSet<Guid> PlayersActed { get; set; } = new();

    // Dealer/SmallBlind/BigBlind positions (index in seat list)
    public int DealerIndex { get; set; } 
    public int SmallBlindIndex { get; set; }
    public int BigBlindIndex { get; set; }
}

public class Table
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Table 1";
    public int MaxSeats { get; set; } = 9;
    public List<Player> Seats { get; set; } = new(); // Use list for now, index = seat number
    public GameState GameState { get; set; } = new();
    public Deck Deck { get; set; } = new();
    
    public Table()
    {
        GameState.TableId = Id;
    }
}

public class Deck
{
    private List<Card> _cards = new();
    private Random _rng = new();

    public Deck()
    {
        Reset();
    }

    public void Reset()
    {
        _cards.Clear();
        foreach (Suit s in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank r in Enum.GetValues(typeof(Rank)))
            {
                _cards.Add(new Card(s, r));
            }
        }
        Shuffle();
    }

    public void Shuffle()
    {
        int n = _cards.Count;
        while (n > 1)
        {
            n--;
            int k = _rng.Next(n + 1);
            Card value = _cards[k];
            _cards[k] = _cards[n];
            _cards[n] = value;
        }
    }

    public Card? Draw()
    {
        if (_cards.Count == 0) return null;
        var card = _cards[0];
        _cards.RemoveAt(0);
        return card;
    }
}
