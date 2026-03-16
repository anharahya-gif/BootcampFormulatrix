using System;
using UnoBoardGame.Models;
using System.Collections.Generic;

namespace UnoBoardGame.UI;

public class ConsoleView
{
    public void RenderGameInfo(
        Deck deck,
        DiscardPile discardPile,
        Card topCard,
        CardColor currentColor,
        Direction direction
    )
    {
        Console.WriteLine("=========== UNO GAME ===========");
        Console.WriteLine($"Deck       : {deck.Count} cards");
        Console.WriteLine($"Discard    : {discardPile.Count} cards");
        Console.WriteLine($"Top Card   : {FormatCard(topCard)}");
        Console.WriteLine($"Current Color : {currentColor}");

        string dirSymbol = direction == Direction.Clockwise ? "→" : "←";
        Console.WriteLine($"Direction  : {direction} {dirSymbol}");
        Console.WriteLine("================================");
    }

    public void ShowPlayerStatus(
        List<Player> players,
        int currentPlayerIndex
    )
    {
        Console.WriteLine("\n===== PLAYER STATUS =====");

        for (int i = 0; i < players.Count; i++)
        {
            var p = players[i];

            string turn = i == currentPlayerIndex ? "👉" : "  ";
            string type = p.IsHuman ? "(YOU)" : "(BOT)";

            Console.WriteLine(
                $"{turn} {p.Name,-10} {type,-6} : {p.GetAllCards().Count} cards"
            );
        }

        Console.WriteLine("=========================");
    }

    public void ShowMessage(string message)
    {
        Console.WriteLine(message);
    }

    public int AskCardChoice()
    {
        Console.Write("Choose card: ");
        return int.Parse(Console.ReadLine()!);
    }

    private string FormatCard(Card card)
    {
        return card.Type switch
        {
            CardType.Number => $"[{card.Color} {card.Number}]",
            CardType.Skip => $"[{card.Color} Skip]",
            CardType.Reverse => $"[{card.Color} Reverse]",
            CardType.DrawTwo => $"[{card.Color} +2]",
            CardType.Wild => "[Wild]",
            CardType.WildDrawFour => "[Wild +4]",
            _ => "[Unknown]"
        };
    }
}
