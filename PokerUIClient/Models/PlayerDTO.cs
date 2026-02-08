
namespace PokerUIClient.Models
{
    public class PlayerDTO
    {
        public string? Name { get; set; }
        public int ChipStack { get; set; }
        public int CurrentBet { get; set; }
        public bool IsFolded { get; set; }
        public int SeatIndex { get; set; }
        public List<string> Hand { get; set; } = new();
        public string State { get; set; } = "Active";
    }
}