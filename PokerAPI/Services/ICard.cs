using PokerAPI.Models;
namespace PokerAPI.Services.Interfaces
{
public interface ICard
{
    Rank Rank { get; }
    Suit Suit { get; }
}
}