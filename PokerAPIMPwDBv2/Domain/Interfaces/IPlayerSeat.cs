namespace PokerAPIMPwDB.Domain.Interfaces
{
    public class IPlayerSeat
    {
        public int SeatIndex { get; set; }
        public IPlayer? Player { get; set; }
    }
}
