using System;
using UnoGame.Game;
using UnoGame.Players;

namespace UnoGame
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "UNO Console Game";
            Console.WriteLine("=== UNO GAME ===\n");

            var game = new GameController();

            // Tambah player (sementara hard-coded)
            game.Players.Add(new Player("Anhar"));
            game.Players.Add(new Player("Bot"));

            // Start game
            game.StartGame();

            Console.WriteLine("Game started successfully!");
            Console.WriteLine($"Players: {string.Join(", ", game.Players.ConvertAll(p => p.Name))}");
            Console.WriteLine($"Current Player: {game.CurrentPlayer.Name}");

            while (!game.IsGameOver)
            {
                game.PlayTurn();
                Console.ReadLine(); // pause tiap turn
            }


            Console.WriteLine("\n(Logic turn & UI akan ditambahkan selanjutnya)");
            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }
    }
}
