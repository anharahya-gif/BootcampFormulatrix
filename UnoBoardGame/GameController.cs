using System;
using System.Collections.Generic;
using System.Linq;
using UnoBoardGame.Models;

namespace UnoBoardGame.Game;

public class GameController
{
    /* ================= STATE ================= */

    public List<Player> Players { get; } = new();
    public Deck Deck { get; } = new();
    public DiscardPile DiscardPile { get; } = new();

    public int CurrentPlayerIndex { get; private set; }
    public Direction Direction { get; private set; } = Direction.Clockwise;
    public CardColor CurrentColor { get; private set; }
    public bool IsGameOver { get; private set; }

    public Player CurrentPlayer => Players[CurrentPlayerIndex];

    /* ================= GAME FLOW ================= */

    public void StartGame()
    {
        BuildDeck();
        Deck.Shuffle();

        foreach (var p in Players)
            for (int i = 0; i < 7; i++)
                AddCardToPlayer(p, DrawFromDeck());

        var firstCard = DrawFromDeck();
        DiscardPile.Add(firstCard);

        CurrentColor = firstCard.Color ?? CardColor.Red;
        CurrentPlayerIndex = 0;
    }

    public void PlayBotTurn()
    {
        var player = CurrentPlayer;

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

    public void PlayHumanCard(Player player, int cardIndex)
    {
        var cards = player.GetAllCards();

        if (cardIndex < 0 || cardIndex >= cards.Count)
            throw new InvalidOperationException("Invalid card index");

        var card = cards[cardIndex];

        if (!CanPlayCard(player, card))
            throw new InvalidOperationException("Card cannot be played");

        PlayCard(player, card);
    }

    public void HumanDraw(Player player)
    {
        AddCardToPlayer(player, DrawFromDeck());
    }

    /* ================= PLAYER ================= */

    public int GetPlayerCardCount(Player player)
        => player.GetAllCards().Count;

    public List<Card> GetPlayerCards(Player player)
        => player.GetAllCards();

    private void AddCardToPlayer(Player player, Card card)
        => player.Cards[card.Type].Add(card);

    private void RemoveCardFromPlayer(Player player, Card card)
        => player.Cards[card.Type].Remove(card);

    /* ================= VALIDATION ================= */

    public bool CanPlayCard(Player player, Card card)
    {
        var top = GetTopDiscard();

        if (card.Type == CardType.Wild || card.Type == CardType.WildDrawFour)
            return true;

        if (card.Color == CurrentColor)
            return true;

        return card.Type == CardType.Number &&
               top.Type == CardType.Number &&
               card.Number == top.Number;
    }

    /* ================= CARD PLAY ================= */

    private void PlayCard(Player player, Card card)
    {
        RemoveCardFromPlayer(player, card);
        DiscardPile.Add(card);

        player.HasCalledUno = false;
        CurrentColor = card.Color ?? ChooseColor(player);

        HandleCardEffect(card);

        if (GetPlayerCardCount(player) == 0)
            EndGame(player);
    }

    private void HandleCardEffect(Card card)
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
                ForceDraw(GetNextPlayer(1), 2);
                MoveToNextPlayer(2);
                break;

            case CardType.WildDrawFour:
                ForceDraw(GetNextPlayer(1), 4);
                MoveToNextPlayer(2);
                break;

            default:
                MoveToNextPlayer(1);
                break;
        }
    }

    /* ================= TURN ================= */

    private void MoveToNextPlayer(int step)
    {
        CurrentPlayerIndex =
            (CurrentPlayerIndex + step * (int)Direction + Players.Count)
            % Players.Count;
    }

    private void ReverseDirection()
    {
        Direction = Direction == Direction.Clockwise
            ? Direction.CounterClockwise
            : Direction.Clockwise;
    }

    private Player GetNextPlayer(int offset)
    {
        int idx =
            (CurrentPlayerIndex + offset * (int)Direction + Players.Count)
            % Players.Count;

        return Players[idx];
    }

    /* ================= DECK ================= */

    public Card DrawFromDeck()
    {
        if (Deck.Count == 0)
            RefillDeckFromDiscard();

        return Deck.Draw();
    }

    private void RefillDeckFromDiscard()
    {
        var top = DiscardPile.PopTop();

        var rest = DiscardPile.PopAll();
        Deck.RefillFrom(rest);
        Deck.Shuffle();

        DiscardPile.Add(top);
    }

    public Card GetTopDiscard()
        => DiscardPile.Top();

    /* ================= UNO ================= */

    public void CallUno(Player player)
        => player.HasCalledUno = true;

    public bool ShouldApplyUnoPenalty(Player player)
        => GetPlayerCardCount(player) == 1 && !player.HasCalledUno;

    public void ApplyUnoPenalty(Player player)
        => ForceDraw(player, 2);

    /* ================= STRATEGY ================= */

    private Card ChooseBestCard(Player player)
    {
        var playable = player.GetAllCards()
            .Where(c => CanPlayCard(player, c))
            .ToList();

        if (!playable.Any())
            return null!;

        var opponent = GetNextPlayer(1);
        bool danger = GetPlayerCardCount(opponent) <= 2;

        var colorCount = player.GetAllCards()
            .Where(c => c.Color != null)
            .GroupBy(c => c.Color!.Value)
            .ToDictionary(g => g.Key, g => g.Count());

        return playable
            .OrderByDescending(c => ScoreCard(c, colorCount, danger))
            .First();
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

    /* ================= UTILS ================= */

    private void ForceDraw(Player player, int count)
    {
        for (int i = 0; i < count; i++)
            AddCardToPlayer(player, DrawFromDeck());
    }

    private CardColor ChooseColor(Player player)
    {
        var colored = player.GetAllCards()
            .Where(c => c.Color != null)
            .ToList();

        if (!colored.Any())
        {
            var colors = Enum.GetValues<CardColor>();
            return colors[new Random().Next(colors.Length)];
        }

        return colored
            .GroupBy(c => c.Color!.Value)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
    }

    private void EndGame(Player winner)
    {
        IsGameOver = true;
    }

    /* ================= INIT ================= */

    private void BuildDeck()
    {
        foreach (CardColor color in Enum.GetValues<CardColor>())
        {
            for (int i = 0; i <= 9; i++)
                Deck.Add(new Card { Type = CardType.Number, Color = color, Number = i });

            Deck.Add(new Card { Type = CardType.Skip, Color = color });
            Deck.Add(new Card { Type = CardType.Reverse, Color = color });
            Deck.Add(new Card { Type = CardType.DrawTwo, Color = color });
        }

        for (int i = 0; i < 4; i++)
        {
            Deck.Add(new Card { Type = CardType.Wild });
            Deck.Add(new Card { Type = CardType.WildDrawFour });
        }
    }
}
