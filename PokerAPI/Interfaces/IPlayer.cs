using PokerAPI.Models;

namespace PokerAPI.Services.Interfaces
{
    public interface IPlayer
    {
        string Name { get; set; }
        int ChipStack { get; set; }
        PlayerState State { get; set; }
        bool IsAllIn { get; }
        int SeatIndex { get; set; }
    }
}
