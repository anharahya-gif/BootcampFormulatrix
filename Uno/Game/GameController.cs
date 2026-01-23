using System.Collections.Generic;
using UnoGame.Cards;
using UnoGame.Core;
using UnoGame.Players;

namespace UnoGame.Game
{
    public class GameController : IGameContext
    {
        public IGameEvents? Events { get; set; }

        public List<Player> Players { get; } = new();
        public Deck Deck { get; } = new();
        public DiscardPile? DiscardPile { get; } = new();

        public int CurrentPlayerIndex { get; private set; }
        public Direction Direction { get; private set; } = Direction.Clockwise;
        public CardColor CurrentColor { get; private set; }
        public bool IsGameOver { get; private set; }

        public Player CurrentPlayer => Players[CurrentPlayerIndex];


        public void StartGame()
        {
            IsGameOver = false;
            CurrentPlayerIndex = 0;

            // 1️⃣ Bagi 7 kartu ke tiap player
            for (int i = 0; i < 7; i++)
            {
                foreach (var player in Players)
                {
                    player.DrawCard(Deck);
                }
            }

            // 2️⃣ Ambil kartu awal untuk discard pile
            Card firstCard;
            do
            {
                firstCard = Deck.Draw();
            }
            while (firstCard is WildDrawFourCard); // aturan UNO (opsional tapi benar)

            DiscardPile.Add(firstCard);

            // 3️⃣ Set warna awal
            if (firstCard.Color.HasValue)
                CurrentColor = firstCard.Color.Value;

            Console.WriteLine($"First card: {firstCard.GetType().Name}");
        }


        public void PlayTurn()
        {
            if (DiscardPile.IsEmpty())
            {
                throw new InvalidOperationException("Discard pile is empty. Game not initialized correctly.");
            }

            var player = CurrentPlayer;

            Console.Clear();
            Console.WriteLine("========================================");
            Console.WriteLine("🎴 UNO GAME");
            Console.WriteLine("========================================\n");

            Console.Write("Top Card : ");
            PrintCard(DiscardPile.Top());
            Console.WriteLine("\n");

            Console.WriteLine($"➡ Current Player : {player.Name}");
            Console.WriteLine($"Direction        : {Direction}");
            Console.WriteLine("\nYour Hand:");

            int index = 1;
            foreach (var card in player.Hand.Cards)
            {
                Console.Write($"[{index++}] ");
                PrintCard(card);
                Console.WriteLine();
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.ReadLine();

            Card? playable = player.Hand.Cards
                .FirstOrDefault(c => c.CanBePlayedOn(DiscardPile.Top()));

            if (playable == null)
            {
                Console.WriteLine($"{player.Name} has no playable card. Drawing...");
                CheckAndRefillDeck();
                player.DrawCard(Deck);
                MoveToNextPlayer();
                return;
            }

            Console.WriteLine($"{player.Name} plays {FormatCard(playable)}");

            player.PlayCard(playable);
            DiscardPile.Add(playable);

            // Apply effect
            if (playable is ActionCard action)
                action.ApplyEffect(this);
            else if (playable is WildCard wild)
            {
                var color = ChooseColorForPlayer(player);
                wild.ChooseColor(color);
                wild.ApplyEffect(this);
            }
            else if (playable is WildDrawFourCard wild4)
            {
                var color = ChooseColorForPlayer(player);
                wild4.ChooseColor(color);
                wild4.ApplyEffect(this);
            }

            CheckUno(player);

            if (player.Hand.IsEmpty())
            {
                EndGame(player);
                return;
            }

            MoveToNextPlayer();
        }


        public void MoveToNextPlayer(int skipCount = 0)
        {
            int step = Direction == Direction.Clockwise ? 1 : -1;
            CurrentPlayerIndex = (CurrentPlayerIndex + step * (skipCount + 1) + Players.Count) % Players.Count;
        }

        public void CheckAndRefillDeck()
        {
            if (!Deck.IsEmpty()) return;

            var cards = DiscardPile.TakeAllExceptTop();
            Deck.AddCards(cards);
            Deck.Shuffle();
        }

        public void EndGame(Player winner)
        {
            IsGameOver = true;
            Events?.OnGameEnded(winner);
        }

        // ===== UNO LOGIC =====
        public void CheckUno(Player player)
        {
            if (player.Hand.Count() == 1 && !player.HasCalledUno)
                ApplyUnoPenalty(player);
        }

        public void ApplyUnoPenalty(Player player)
        {
            player.DrawCard(Deck);
            player.DrawCard(Deck);
            Events?.OnUnoPenalty(player);
        }

        // ===== IGameContext IMPLEMENTATION =====
        public void SkipNextPlayer()
        {
            MoveToNextPlayer(1);
        }

        public void ReverseDirection()
        {
            Direction = Direction == Direction.Clockwise
                ? Direction.CounterClockwise
                : Direction.Clockwise;

            Events?.OnDirectionChanged(Direction);
        }

        public void ForceDraw(Player player, int count)
        {
            for (int i = 0; i < count; i++)
            {
                CheckAndRefillDeck();
                player.DrawCard(Deck);
            }
        }

        public void SetCurrentColor(CardColor color)
        {
            CurrentColor = color;
        }

        private CardColor ChooseColorForPlayer(Player player)
        {
            // sementara: random
            var colors = Enum.GetValues<CardColor>();
            var chosen = colors[new Random().Next(colors.Length)];

            Console.WriteLine($"{player.Name} chooses color {chosen}");
            return chosen;
        }

        private string FormatCard(Card card)
        {
            return card switch
            {
                NumberCard n => $"{n.Color} {n.Number}",
                SkipCard s => $"{s.Color} SKIP",
                ReverseCard r => $"{r.Color} REVERSE",
                DrawTwoCard d => $"{d.Color} +2",
                WildCard => "WILD",
                WildDrawFourCard => "WILD +4",
                _ => card.GetType().Name
            };
        }
        private void WriteColor(CardColor color, string text)
        {
            var prev = Console.ForegroundColor;

            Console.ForegroundColor = color switch
            {
                CardColor.Red => ConsoleColor.Red,
                CardColor.Yellow => ConsoleColor.Yellow,
                CardColor.Green => ConsoleColor.Green,
                CardColor.Blue => ConsoleColor.Blue,
                _ => ConsoleColor.White
            };

            Console.Write(text);
            Console.ForegroundColor = prev;
        }
        private string CardIcon(Card card)
        {
            return card switch
            {
                NumberCard => "🔢",
                SkipCard => "⏭",
                ReverseCard => "🔁",
                DrawTwoCard => "+2",
                WildCard => "🌈",
                WildDrawFourCard => "+4",
                _ => "?"
            };
        }
        private void PrintCard(Card card)
        {
            if (card is WildCard)
            {
                Console.Write("🌈 WILD");
                return;
            }

            if (card is WildDrawFourCard)
            {
                Console.Write("🌈 WILD +4");
                return;
            }

            if (card.Color.HasValue)
            {
                WriteColor(card.Color.Value, $"{CardIcon(card)} {card.Color}");
                Console.Write(" ");

                Console.Write(card switch
                {
                    NumberCard n => n.Number.ToString(),
                    SkipCard => "SKIP",
                    ReverseCard => "REVERSE",
                    DrawTwoCard => "+2",
                    _ => ""
                });
            }
        }



    }
}
