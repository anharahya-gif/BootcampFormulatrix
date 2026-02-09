using System.Collections.Generic;

namespace PokerAPI.DTOs
{
    public class ShowdownPlayerDto
    {
        public string Name { get; set; } = string.Empty;
        public int SeatIndex { get; set; }
        public List<string> Hand { get; set; } = new();
        public string HandRank { get; set; } = string.Empty;
        public int ChipStack { get; set; }
        public bool IsFolded { get; set; }
        public bool IsWinner { get; set; }
        public int Winnings { get; set; }
    }
}
