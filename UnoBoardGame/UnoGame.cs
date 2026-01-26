// using System;
// using System.Collections.Generic;

// public class UnoGame
// {
//     private List<Player> players = new();
//     private Deck deck = new();
//     private Card topCard;
//     private int direction = 1;

//     private int currentIndex = 0;

//     public void Start()
//     {
//         SetupPlayers();
//         DealCards();

//         topCard = deck.Draw();
//         deck.Discard(topCard);
//         Console.WriteLine($"Kartu awal: {topCard}");

//         while (true)
//         {
//             var player = players[currentIndex];

//             var playedCard = player.TakeTurn(topCard, deck);

//             if (playedCard != null)
//             {
//                 PlayCard(player, playedCard);
//                 topCard = playedCard;

//                 if (player.Hand.Count == 1 && !player.HasCalledUno)
//                 {
//                     Console.WriteLine($"{player.Name} lupa bilang UNO! +2 kartu");
//                     player.DrawCard(deck);
//                     player.DrawCard(deck);
//                 }

//                 if (player.Hand.Count == 0)
//                 {
//                     Console.WriteLine($"\n🎉 {player.Name} MENANG!");
//                     break;
//                 }
//             }

//             NextTurn();
//         }
//     }


//     private void SetupPlayers()
//     {
//         Console.Write("Masukkan jumlah pemain: ");
//         int totalPlayers = ReadInt("Masukkan jumlah pemain (2-10): ", 2, 10);

//         for (int i = 1; i <= totalPlayers; i++)
//         {
//             Console.WriteLine($"\nPemain {i}");
//             Console.Write("Nama: ");
//             string name = Console.ReadLine();


//             string isBot;
//             while (true)
//             {
//                 Console.Write("Apakah BOT? (y/n): ");
//                 isBot = Console.ReadLine().ToLower();

//                 if (isBot == "y" || isBot == "n")
//                     break;

//                 Console.WriteLine("Input harus 'y' atau 'n'!");
//             }

//             if (isBot == "y")
//                 players.Add(new BotPlayer(name));
//             else
//                 players.Add(new HumanPlayer(name));
//         }
//     }
//     private string ChooseColor()
// {
//     string[] colors = { "Red", "Blue", "Green", "Yellow" };
//     return colors[new Random().Next(colors.Length)];
// }

//     private void ApplyAction(Card card)
// {
//     if (card is WildCard wild)
//     {
//         wild.ChooseColor(ChooseColor());
//     }

//     switch (card.Type)
//     {
//         case CardType.Skip:
//             Console.WriteLine("⏭ Skip!");
//             NextTurn();
//             break;

//         case CardType.Reverse:
//             Console.WriteLine("🔄 Reverse!");
//             direction *= -1;
//             break;

//         case CardType.DrawTwo:
//             Console.WriteLine("➕2!");
//             NextTurn();
//             players[currentIndex].DrawCard(deck);
//             players[currentIndex].DrawCard(deck);
//             break;

//         case CardType.WildDrawFour:
//             Console.WriteLine("➕4!");
//             NextTurn();
//             for (int i = 0; i < 4; i++)
//                 players[currentIndex].DrawCard(deck);
//             break;
//     }
// }



//     private void DealCards()
//     {
//         foreach (var p in players)
//         {
//             for (int i = 0; i < 5; i++)
//                 p.DrawCard(deck);
//         }
//     }

//     private void NextTurn()
//     {
//         currentIndex++;
//         if (currentIndex >= players.Count)
//             currentIndex = 0;
//     }
//     private int ReadInt(string message, int min, int max)
//     {
//         int value;
//         while (true)
//         {
//             Console.Write(message);
//             string input = Console.ReadLine();

//             if (!int.TryParse(input, out value))
//             {
//                 Console.WriteLine("Input harus berupa angka!");
//                 continue;
//             }

//             if (value < min || value > max)
//             {
//                 Console.WriteLine($"Input harus antara {min} dan {max}!");
//                 continue;
//             }

//             return value;
//         }
//     }
//     public void PlayCard(Player player, Card card)
//     {
//         player.Hand.Remove(card);
//         deck.Discard(card);
//     }


// }

