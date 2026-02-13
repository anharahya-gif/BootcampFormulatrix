using PokerAPIMPwDB.Domain.Enums;

namespace PokerAPIMPwDB.Domain.Interfaces
{
    public interface ICard
    {
        Rank Rank { get; }
        Suit Suit { get; }
    }
}
