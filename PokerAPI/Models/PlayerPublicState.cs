namespace PokerAPI.Models.DTOs
{
    public class PlayerPublicState
    {
        public int SeatIndex { get; set; }
        public string Name { get; set; } = "";
        public int ChipStack { get; set; }
        public string State { get; set; } = "";
        public int CurrentBet { get; set; }

        public bool IsFolded { get; set; }
        public List<string> Hand { get; set; } = new();
    }
}
