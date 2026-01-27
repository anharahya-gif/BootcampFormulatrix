using UnoBoardGame.Game;
using UnoBoardGame.UI;
using UnoBoardGame.Models;

// var game = new GameController();
// var view = new ConsoleView();

// // init player, bot, dll
// game.StartGame();

// while (!game.IsGameOver)
// {
//     var player = game.Players[game.CurrentPlayerIndex];

//     view.ShowPlayerStatus(game.Players, game.CurrentPlayerIndex);
//     view.RenderGameInfo(
//         game.Deck,
//         game.DiscardPile,
//         game.GetTopDiscard(),
//         game.CurrentColor,
//         game.Direction
//     );

//     if (player.IsHuman)
//     {
//         var cards = game.GetHumanPlayableCards(player);

//         for (int i = 0; i < cards.Count; i++)
//             Console.WriteLine($"{i + 1}. {cards[i]}");

//         Console.WriteLine("0. Draw card");

//         int choice = view.AskCardChoice();

//         if (choice == 0)
//             game.AddCardToPlayer(player, game.DrawFromDeck());
//         else
//             game.PlayHumanCard(player, choice - 1);
//     }
//     else
//     {
//         game.BotTurn(player);
//     }
// }



