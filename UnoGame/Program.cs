using System;
using System.Collections.Generic;
using UnoGame.Model;
using UnoGame.View;
using UnoGame.Controller;

namespace UnoGame
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "UNO Console Game";

            // =======================
            // CREATE PLAYERS
            // =======================
            var players = CreatePlayers();

            // =======================
            // CREATE CORE OBJECTS
            // =======================
            var deck = new Deck();
            var discardPile = new DiscardPile();
            var view = new ConsoleGameView();

            // =======================
            // CREATE CONTROLLER
            // =======================
            var controller = new GameController(
                players,
                deck,
                discardPile,
                view,
                view
            );

            // =======================
            // START GAME
            // =======================
            controller.StartGame();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        static List<Player> CreatePlayers()
        {
            int totalPlayers;

            while (true)
            {
                Console.Write("Jumlah pemain (2 - 10): ");
                if (int.TryParse(Console.ReadLine(), out totalPlayers)
                    && totalPlayers >= 2 && totalPlayers <= 10)
                    break;

                Console.WriteLine("Input tidak valid!");
            }

            var players = new List<Player>();

            // Human selalu pemain pertama
            players.Add(new HumanPlayer("You"));

            for (int i = 2; i <= totalPlayers; i++)
            {
                players.Add(new BotPlayer($"Bot-{i - 1}"));
            }

            return players;
        }

    }
}
