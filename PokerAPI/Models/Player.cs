namespace PokerAPI.Models
{
    public class Player
    {
        public string Name { get; set; }
        public int ChipStack { get; set; }

        public Player(string name, int startingChips = 1000)
        {
            Name = name;
            ChipStack = startingChips;
        }
        public PlayerState State { get; set; } = PlayerState.Active;
        public bool IsAllIn => State == PlayerState.AllIn;
    }
}
