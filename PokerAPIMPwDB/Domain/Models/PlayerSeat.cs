using PokerAPIMPwDB.Domain.Interfaces;

namespace PokerAPIMPwDB.Domain.Models
{
    public class PlayerSeat
    {
        public int SeatIndex { get; }

        public IPlayer? Player { get; private set; }

        public int Chips { get; private set; }

        public bool IsFolded { get; private set; }
        public bool IsAllIn { get; private set; }

        public bool IsOccupied => Player != null;

        public PlayerSeat(int seatIndex)
        {
            SeatIndex = seatIndex;
        }

        public void SitDown(IPlayer player, int chips)
        {
            if (IsOccupied)
                throw new InvalidOperationException("Seat already occupied");

            if (chips <= 0)
                throw new InvalidOperationException("Invalid chip amount");

            Player = player;
            Chips = chips;
            IsFolded = false;
            IsAllIn = false;
        }

        public void Leave()
        {
            Player = null;
            Chips = 0;
            IsFolded = false;
            IsAllIn = false;
        }

        public void Fold()
        {
            IsFolded = true;
        }

        public void AllIn()
        {
            IsAllIn = true;
        }
    }
}
