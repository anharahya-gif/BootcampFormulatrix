// public class BotPlayer : Player
// {
//     public BotPlayer(string name) : base(name) { }

//     public override Card TakeTurn(Card topCard, Deck deck)
//     {
//         Console.WriteLine($"\nGiliran: {Name}");

//         foreach (var card in Hand)
//         {
//             if (card.CanPlayOn(topCard))
//             {
//                 Hand.Remove(card);
//                 Console.WriteLine($"{Name} memainkan {card}");

//                 if (Hand.Count == 1)
//                     CallUno();

//                 return card;
//             }
//         }

//         DrawCard(deck);
//         Console.WriteLine($"{Name} draw kartu");
//         return null;
//     }
// }
