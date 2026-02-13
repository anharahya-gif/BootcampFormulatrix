using PokerAPIMPwDB.Domain.Enums;
using PokerAPIMPwDB.Domain.Interfaces;

namespace PokerAPIMPwDB.Domain.Models
{
    public class Card : ICard
    {
        public Rank Rank { get; private set; }
        public Suit Suit { get; private set; }

        public Card(Rank rank, Suit suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public override string ToString()
        {
            return $"{Rank} of {Suit}";
        }
    }
}
