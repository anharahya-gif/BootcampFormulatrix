namespace PokerAPI.Models
{
    public class Player
    {
        public string Name { get; set; }
        public int ChipStack { get; set; }

        public Player(string name, int startingChips = 1000,int seatIndex=-1)
        {
            Name = name;
            ChipStack = startingChips;
            SeatIndex = SeatIndex;
        }
        public PlayerState State { get; set; } = PlayerState.Active;
        public bool IsAllIn => State == PlayerState.AllIn;
        public int SeatIndex { get; set; } 
    }
}
