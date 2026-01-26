// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");
using UnoBoardGame.Models;
class Program
{
    static void Main()
    {
        Console.WriteLine("=== UNO GAME ===");
        Console.Write("Masukkan jumlah pemain (2–4): ");
        int totalPlayers = int.Parse(Console.ReadLine()!);

        var game = new GameController();

        game.Players.Add(new Player("YOU", true));

        for (int i = 2; i <= totalPlayers; i++)
            game.Players.Add(new Player($"BOT {i}", false));

        game.StartGame();

        while (!game.IsGameOver)
            game.PlayTurn();
    }
}
