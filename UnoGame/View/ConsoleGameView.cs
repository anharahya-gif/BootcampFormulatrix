using System;
using System.Collections.Generic;
using System.Linq;
using UnoGame.Model;

namespace UnoGame.View
{
    public class ConsoleGameView : IGameView, IGameInput
    {
        private int _selectedIndex = 0;

        /* =======================
           GAME FLOW DISPLAY
        ======================= */
        private int _handStartRow = -1;




        public void ShowGameStart(List<Player> players)
        {
            Console.Clear();
            Console.WriteLine("════════════ UNO GAME ════════════");
            Console.WriteLine("Players:");

            foreach (var p in players)
                Console.WriteLine($"- {p.Name}");

            Console.WriteLine("══════════════════════════════════");
            Console.WriteLine("Press any key to start...");
            Console.ReadKey();
        }

        public void ShowGameStatus(
            int deckCount,
            int discardCount,
            Dictionary<Player, int> playerCardCounts,
            Card? lastPlayedCard,
            Card topCard,
            CardColor currentColor,
            Player? lastPlayer,
            Player currentPlayer,
            Player nextPlayer,
            Direction direction
        )
        {
            Console.Clear();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("UNO GAME");
            Console.ResetColor();

            // ===== STATUS BAR =====
            Console.WriteLine(
                $"Deck: {deckCount} | " +
                $"Discard: {discardCount} | " +
                $"Direction: {direction} | " +
                $"Color: {currentColor}"
            );

            // ===== TURN INFO =====
            string lastInfo = (lastPlayer != null && lastPlayedCard != null)
                ? $"{lastPlayer.Name} → {FormatCard(lastPlayedCard)}"
                : "-";

            Console.WriteLine(
                $"Last: {lastInfo} | " +
                $"Current: {currentPlayer.Name} | " +
                $"Next: {nextPlayer.Name}"
            );

            // ===== PLAYER COUNTS =====
            Console.WriteLine(
                "Players: " +
                string.Join(" | ",
                    playerCardCounts.Select(p =>
                        $"{p.Key.Name}({p.Value})"
                    )
                )
            );

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("TOP CARD");
            RenderTopCard(topCard, currentColor);
            Console.WriteLine();
        }
        public void ShowPlayerHand(Player player)
        {
            // intentionally left empty
            // Hand is rendered via ASCII in ChooseCard()
        }


        public void ShowPlayerTurn(Player player)
        {
            Console.WriteLine($"=== {player.Name}'s TURN ===");
        }

        public void ShowCardPlayed(Player player, Card card)
        {
            Console.WriteLine($"{player.Name} played {FormatCard(card)}");
        }

        public void ShowCardDrawn(Player player, Card card)
        {
            Console.WriteLine($"{player.Name} drew {FormatCard(card)}");
        }

        public void ShowUnoCalled(Player player)
        {
            Console.WriteLine($"{player.Name} calls UNO!");
        }

        public void ShowUnoPenalty(Player player)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{player.Name} failed to call UNO! Penalty applied.");
            Console.ResetColor();
        }

        public void ShowPenalty(Player player, string reason, int drawCount)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine();
            Console.WriteLine("⚠️  PENALTY!");
            Console.WriteLine($"{player.Name} terkena penalty!");
            Console.WriteLine($"Reason : {reason}");
            Console.WriteLine($"Draw   : {drawCount} card(s)");
            Console.ResetColor();
        }

        public void ShowDirectionChanged(Direction direction)
        {
            Console.WriteLine($"Direction changed to {direction}");
        }

        public void ShowGameOver(Player winner)
        {
            Console.WriteLine();
            Console.WriteLine("=== GAME OVER ===");
            Console.WriteLine($"Winner: {winner.Name}");
        }

        /* =======================
           INPUT (CARD SELECTION)
        ======================= */

        public Card? ChooseCard(Player player)
        {
            var cards = player.Cards.SelectMany(c => c.Value).ToList();

            if (cards.Count == 0)
                return null;

            _selectedIndex = Math.Clamp(_selectedIndex, 0, cards.Count - 1);

            while (true)
            {
                RenderHand(cards);

                var key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow:
                        _selectedIndex = Math.Max(0, _selectedIndex - 1);
                        break;

                    case ConsoleKey.RightArrow:
                        _selectedIndex = Math.Min(cards.Count - 1, _selectedIndex + 1);
                        break;

                    case ConsoleKey.Enter:
                        return cards[_selectedIndex];

                    case ConsoleKey.Spacebar:
                        return null; // DRAW

                    case ConsoleKey.Escape:
                        return null;

                    default:
                        ShowInputError("Gunakan ← → Enter atau Space");
                        break;
                }
            }
        }


        public CardColor ChooseColor(Player player)
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("Choose Color:");
                Console.WriteLine("[0] Red   [1] Yellow   [2] Green   [3] Blue");
                Console.Write("Input: ");

                var input = Console.ReadLine();

                if (int.TryParse(input, out int value) && value >= 0 && value <= 3)
                {
                    return (CardColor)value;
                }

                ShowInputError("Input tidak valid! Masukkan angka 0 - 3");
            }
        }

        public bool CallUno(Player player)
        {
            Console.Write("Call UNO? (y/n): ");
            return Console.ReadLine()?.ToLower() == "y";
        }

        /* =======================
           ASCII CARD RENDER
        ======================= */

        private void RenderHand(List<Card> cards)
        {
            if (_handStartRow == -1)
                _handStartRow = Console.CursorTop;

            int cardsPerRow = CalculateCardsPerRow();
            int rows = (int)Math.Ceiling(cards.Count / (double)cardsPerRow);

            int clearLines = 3 + rows * 8; // header + kartu
            ClearHandArea(clearLines);

            Console.SetCursorPosition(0, _handStartRow);
            
            Console.WriteLine("Use ← → to select | Enter = Play | Space = Draw");
            Console.WriteLine();

            for (int i = 0; i < cards.Count; i += cardsPerRow)
            {
                var rowCards = cards.Skip(i).Take(cardsPerRow).ToList();
                var rendered = rowCards
                    .Select((c, idx) =>
                        RenderCard(c, i + idx == _selectedIndex))
                    .ToList();

                for (int line = 0; line < 7; line++)
                {
                    for (int c = 0; c < rendered.Count; c++)
                    {
                        bool selected = (i + c == _selectedIndex);
                        Console.ForegroundColor = selected
                            ? ConsoleColor.White
                            : GetConsoleColor(rowCards[c].Color);

                        Console.Write(rendered[c][line]);
                        Console.Write("  ");
                        Console.ResetColor();
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }
        }





        private string[] RenderCard(Card card, bool selected)
        {
            const int innerWidth = 9;

            string color = card.Color?.ToString().ToUpper() ?? "WILD";

            string value = card.Type switch
            {
                CardType.Number => card.Number!.ToString(),
                CardType.Skip => "SKIP",
                CardType.Reverse => "REV",
                CardType.DrawTwo => "+2",
                CardType.Wild => "WILD",
                CardType.WildDrawFour => "+4",
                _ => "?"
            };

            char corner = selected ? '#' : '+';
            char h = '-';
            char v = '|';

            return new[]
            {
        $"{corner}{new string(h, innerWidth)}{corner}",
        $"{v}{CenterText(color, innerWidth)}{v}",
        $"{v}{new string(' ', innerWidth)}{v}",
        $"{v}{CenterText(value, innerWidth)}{v}",
        $"{v}{new string(' ', innerWidth)}{v}",
        $"{v}{CenterText(color, innerWidth)}{v}",
        $"{corner}{new string(h, innerWidth)}{corner}",
    };
        }




        private string FormatCard(Card card)
        {
            if (card.Type == CardType.Number)
                return $"{card.Color} {card.Number}";

            if (card.Color == null)
                return card.Type.ToString();

            return $"{card.Color} {card.Type}";
        }
        private void ClearHandArea(int totalLines)
        {
            for (int i = 0; i < totalLines; i++)
            {
                Console.SetCursorPosition(0, _handStartRow + i);
                Console.Write(new string(' ', Console.WindowWidth));
            }

            Console.SetCursorPosition(0, _handStartRow);
        }

        private int CalculateHandHeight(int cardCount)
        {
            const int cardHeight = 7;      // tinggi kartu besar
            const int headerHeight = 3;    // "Your Cards" + instruksi + empty line
            const int spacing = 1;         // spasi antar row
            const int cardsPerRow = 4;     // HARUS SAMA dengan RenderHand

            int rows = (int)Math.Ceiling(cardCount / (double)cardsPerRow);

            return headerHeight + rows * (cardHeight + spacing);
        }


        private ConsoleColor GetConsoleColor(CardColor? color)
        {
            return color switch
            {
                CardColor.Red => ConsoleColor.Red,
                CardColor.Yellow => ConsoleColor.Yellow,
                CardColor.Green => ConsoleColor.Green,
                CardColor.Blue => ConsoleColor.Cyan,
                _ => ConsoleColor.Magenta // WILD
            };
        }

        private string CenterText(string text, int width)
        {
            if (text.Length >= width)
                return text[..width];

            int left = (width - text.Length) / 2;
            int right = width - text.Length - left;

            return new string(' ', left) + text + new string(' ', right);
        }
        private int CalculateCardsPerRow()
        {
            const int cardWidth = 11;
            const int gap = 2;
            const int maxPerRow = 7;

            int availableWidth = Console.WindowWidth;
            int perRow = (availableWidth + gap) / (cardWidth + gap);

            return Math.Max(1, Math.Min(maxPerRow, perRow));
        }
        private void RenderTopCard(Card card, CardColor currentColor)
        {
            const int innerWidth = 11;
            const int cardWidth = innerWidth + 2;
            const int cardHeight = 7;

            string colorText = currentColor.ToString().ToUpper();

            string value = card.Type switch
            {
                CardType.Number => card.Number!.ToString(),
                CardType.Skip => "SKIP",
                CardType.Reverse => "REV",
                CardType.DrawTwo => "+2",
                CardType.Wild => "WILD",
                CardType.WildDrawFour => "+4",
                _ => "?"
            };

            int left = Math.Max(0, (Console.WindowWidth - cardWidth) / 2);
            int top = Console.CursorTop + 1;

            Console.ForegroundColor = GetConsoleColor(currentColor);

            Console.SetCursorPosition(left, top++);
            Console.WriteLine("+" + new string('-', innerWidth) + "+");

            Console.SetCursorPosition(left, top++);
            Console.WriteLine("|" + CenterText(colorText, innerWidth) + "|");

            Console.SetCursorPosition(left, top++);
            Console.WriteLine("|" + new string(' ', innerWidth) + "|");

            Console.SetCursorPosition(left, top++);
            Console.WriteLine("|" + CenterText(value, innerWidth) + "|");

            Console.SetCursorPosition(left, top++);
            Console.WriteLine("|" + new string(' ', innerWidth) + "|");

            Console.SetCursorPosition(left, top++);
            Console.WriteLine("|" + CenterText(colorText, innerWidth) + "|");

            Console.SetCursorPosition(left, top);
            Console.WriteLine("+" + new string('-', innerWidth) + "+");

            Console.ResetColor();
        }
        private void ShowInputError(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write("Invalid Input: ");
            Console.ResetColor();
            Console.WriteLine(message);
        }


    }

}
