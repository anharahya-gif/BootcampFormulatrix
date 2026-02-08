using PokerAPIMPwDB.Domain.Interfaces;
namespace PokerAPIMPwDB.Domain.Models
{
    public class PlayerSeat
    {
        public int SeatIndex { get; set; }
        public IPlayer Player { get; set; }
    }
}
