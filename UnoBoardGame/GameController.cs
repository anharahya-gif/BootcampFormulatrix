using System;
using System.Collections.Generic;
using System.Linq;
namespace UnoBoardGame.Models;

public class GameController
{
    public List<Player> Players { get; set; } = new();
    public Deck Deck { get; set; } = new();
    public DiscardPile DiscardPile { get; set; } = new();
    public int CurrentPlayerIndex { get; set; }
    public Direction Direction { get; set; } = Direction.Clockwise;
    public CardColor CurrentColor { get; set; }
    public bool IsGameOver { get; set; }

    /* ================= GAME FLOW ================= */

    public void StartGame()
    {
        BuildDeck();
        Deck.Shuffle();

        foreach (var p in Players)
            for (int i = 0; i < 7; i++)
                AddCardToPlayer(p, Deck.Draw());

        var firstCard = Deck.Draw();
        DiscardPile.Cards.Push(firstCard);
        CurrentColor = firstCard.Color ?? CardColor.Red;

        Console.WriteLine($"Game start! First card: {firstCard}");
    }

    public void PlayTurn()
    {
        if (IsGameOver) return;

        ShowPlayerStatus();

        var player = Players[CurrentPlayerIndex];

        // ================= RENDER STATE =================
        Console.WriteLine("\n==================================");
        Console.WriteLine($"TURN : {player.Name}");
        Console.WriteLine($"Top Card     : {GetTopDiscard()}");
        Console.WriteLine($"Current Color: {CurrentColor}");
        Console.WriteLine($"Your Cards   : {GetPlayerCardCount(player)}");
        Console.WriteLine("==================================");

        // ================= PLAYER ACTION =================
        if (player.IsHuman)
        {
            HumanTurn(player);
        }
        else
        {
            BotTurn(player);
        }

        // ================= POST TURN =================
        // Jika player habis kartu → game selesai
        if (GetPlayerCardCount(player) == 0)
        {
            EndGame(player);
            return;
        }

        // Info state setelah turn (debug / UI)
        Console.WriteLine($"➡️ Top Card NOW : {GetTopDiscard()}");
        Console.WriteLine($"➡️ Next Player : {Players[CurrentPlayerIndex].Name}");
    }



    private void HumanTurn(Player player)
    {
        var cards = player.GetAllCards();

        Console.WriteLine("Your Cards:");
        for (int i = 0; i < cards.Count; i++)
            Console.WriteLine($"{i + 1}. {cards[i]}");

        Console.WriteLine("0. Draw card");
        Console.Write("Choose card: ");

        int choice = int.Parse(Console.ReadLine()!);

        if (choice == 0)
        {
            AddCardToPlayer(player, DrawFromDeck());
            return; // ❗ JANGAN MoveToNextPlayer
        }

        var selectedCard = cards[choice - 1];

        if (!CanPlayCard(player, selectedCard))
        {
            Console.WriteLine("❌ Cannot play that card!");
            return;
        }

        PlayCard(player, selectedCard);

        if (GetPlayerCardCount(player) == 1)
        {
            Console.Write("Type 'UNO' to call UNO: ");
            if (Console.ReadLine()?.ToUpper() == "UNO")
                CallUno(player);
        }
    }

    private void BotTurn(Player player)
    {
        var card = ChooseBestCard(player);

        if (card == null)
        {
            AddCardToPlayer(player, DrawFromDeck());
            return;
        }

        PlayCard(player, card);

        if (GetPlayerCardCount(player) == 1)
            CallUno(player);
    }


    public void EndGame(Player winner)
    {
        IsGameOver = true;
        Console.WriteLine($"\n🎉 {winner.Name} WINS!");
    }

    /* ================= PLAYER CARD ================= */

    public void AddCardToPlayer(Player player, Card card)
    {
        player.Cards[card.Type].Add(card);
    }

    public void RemoveCardFromPlayer(Player player, Card card)
    {
        player.Cards[card.Type].Remove(card);
    }

    public List<Card> GetPlayerCards(Player player)
        => player.GetAllCards();

    public int GetPlayerCardCount(Player player)
        => player.GetAllCards().Count;

    /* ================= VALIDATION ================= */

    public bool CanPlayCard(Player player, Card card)
    {
        var top = GetTopDiscard();

        if (card.Type == CardType.Wild || card.Type == CardType.WildDrawFour)
            return true;

        if (card.Color == CurrentColor)
            return true;

        if (card.Type == top.Type && card.Type == CardType.Number && card.Number == top.Number)
            return true;

        return false;
    }

    public bool HasColorCard(Player player, CardColor color)
        => player.GetAllCards().Any(c => c.Color == color);

    /* ================= CARD EFFECT ================= */

    public void PlayCard(Player player, Card card)
    {
        RemoveCardFromPlayer(player, card);
        AddCardToDiscard(card);

        player.HasCalledUno = false;

        Console.WriteLine($"{player.Name} plays {card}");

        CurrentColor = card.Color ?? ChooseColor(player);

        HandleCardEffect(card); // 🔥 SATU-SATUNYA YANG MENGUBAH TURN
    }


    public void HandleCardEffect(Card card)
    {
        switch (card.Type)
        {
            case CardType.Skip:
                MoveToNextPlayer(2);
                break;
            case CardType.Reverse:
                ReverseDirection();
                MoveToNextPlayer(1);
                break;
            case CardType.DrawTwo:
                var p = GetNextPlayer(1);
                AddCardToPlayer(p, DrawFromDeck());
                AddCardToPlayer(p, DrawFromDeck());
                MoveToNextPlayer(2);
                break;
            case CardType.WildDrawFour:
                var p4 = GetNextPlayer(1);
                for (int i = 0; i < 4; i++)
                    AddCardToPlayer(p4, DrawFromDeck());
                MoveToNextPlayer(2);
                break;
            default:
                MoveToNextPlayer(1);
                break;
        }
    }

    public CardColor ChooseColor(Player player)
    {
        var coloredCards = player.GetAllCards()
            .Where(c => c.Color != null)
            .ToList();

        // ❗ Kalau player cuma punya Wild semua
        if (!coloredCards.Any())
        {
            // fallback: random color
            var colors = Enum.GetValues<CardColor>();
            return colors[new Random().Next(colors.Length)];
        }

        return coloredCards
            .GroupBy(c => c.Color)
            .OrderByDescending(g => g.Count())
            .First()
            .Key!.Value;
    }


    /* ================= TURN ================= */

    public void MoveToNextPlayer(int skip)
    {
        CurrentPlayerIndex =
            (CurrentPlayerIndex + skip * (int)Direction + Players.Count)
            % Players.Count;
    }

    public void ReverseDirection()
    {
        Direction = (Direction == Direction.Clockwise)
            ? Direction.CounterClockwise
            : Direction.Clockwise;
    }

    public Player GetNextPlayer(int offset)
    {
        int idx = (CurrentPlayerIndex + offset * (int)Direction + Players.Count)
                  % Players.Count;
        return Players[idx];
    }

    /* ================= DECK ================= */

    public Card DrawFromDeck()
    {
        if (Deck.Cards.Count == 0)
            RefillDeckFromDiscard();

        return Deck.Draw();
    }

    public void AddCardToDiscard(Card card)
    {
        DiscardPile.Cards.Push(card);
    }

    public Card GetTopDiscard()
        => DiscardPile.Top();

    public void RefillDeckFromDiscard()
    {
        var top = DiscardPile.Cards.Pop();

        var rest = DiscardPile.Cards.ToList();
        DiscardPile.Cards.Clear();
        DiscardPile.Cards.Push(top);

        Deck.RefillFrom(rest);
        Deck.Shuffle();
    }


    /* ================= UNO ================= */

    public void CallUno(Player player)
    {
        player.HasCalledUno = true;
    }

    public void CheckUno(Player player)
    {
        if (GetPlayerCardCount(player) == 1 && !player.HasCalledUno)
            ApplyUnoPenalty(player);
    }

    public void ApplyUnoPenalty(Player player)
    {
        Console.WriteLine($"{player.Name} forgot UNO! +2 cards");
        AddCardToPlayer(player, DrawFromDeck());
        AddCardToPlayer(player, DrawFromDeck());
    }

    /* ================= STRATEGY ================= */
    private Card ChooseBestCard(Player player)
    {
        var playableCards = player.GetAllCards()
            .Where(c => CanPlayCard(player, c))
            .ToList();

        if (!playableCards.Any())
            return null!;

        var opponent = GetNextPlayer(1);
        bool opponentDanger = GetPlayerCardCount(opponent) <= 2;

        var colorCount = player.GetAllCards()
            .Where(c => c.Color != null)
            .GroupBy(c => c.Color)
            .ToDictionary(g => g.Key!.Value, g => g.Count());

        Card bestCard = playableCards
            .OrderByDescending(card => ScoreCard(card, colorCount, opponentDanger))
            .First();

        return bestCard;
    }

    private int ScoreCard(
    Card card,
    Dictionary<CardColor, int> colorCount,
    bool opponentDanger)
    {
        int score = card.Type switch
        {
            CardType.Number => 1,
            CardType.Reverse => 3,
            CardType.Skip => 4,
            CardType.DrawTwo => 6,
            CardType.Wild => 7,
            CardType.WildDrawFour => 9,
            _ => 0
        };

        if (card.Color != null && colorCount.TryGetValue(card.Color.Value, out int count))
            score += count;

        if (opponentDanger &&
            (card.Type == CardType.Skip ||
             card.Type == CardType.DrawTwo ||
             card.Type == CardType.WildDrawFour))
            score += 5;

        return score;
    }

    /* ================= INIT ================= */

    private void BuildDeck()
    {
        foreach (CardColor color in Enum.GetValues(typeof(CardColor)))
        {
            for (int i = 0; i <= 9; i++)
                Deck.Cards.Push(new Card { Type = CardType.Number, Color = color, Number = i });

            Deck.Cards.Push(new Card { Type = CardType.Skip, Color = color });
            Deck.Cards.Push(new Card { Type = CardType.Reverse, Color = color });
            Deck.Cards.Push(new Card { Type = CardType.DrawTwo, Color = color });
        }

        for (int i = 0; i < 4; i++)
        {
            Deck.Cards.Push(new Card { Type = CardType.Wild });
            Deck.Cards.Push(new Card { Type = CardType.WildDrawFour });
        }
    }
    private void ShowPlayerStatus()
{
    Console.WriteLine("\n===== PLAYER STATUS =====");

    for (int i = 0; i < Players.Count; i++)
    {
        var p = Players[i];

        string turnMarker = (i == CurrentPlayerIndex) ? "👉" : "  ";
        string type = p.IsHuman ? "(YOU)" : "(BOT)";

        Console.WriteLine(
            $"{turnMarker} {p.Name,-10} {type,-6} : {GetPlayerCardCount(p)} cards"
        );
    }

    Console.WriteLine("=========================");
}

}
