using System.Collections.Generic;

namespace PokerAPI.DTOs
{
    public class PlayerPublicStateDto
    {
        public string Name { get; set; } = string.Empty;
        public int ChipStack { get; set; }
        public int CurrentBet { get; set; }
        public bool IsFolded { get; set; }
        public int SeatIndex { get; set; }
        public string State { get; set; } = string.Empty;
        public List<string> Hand { get; set; } = new();
        public string PossibleHandRank { get; set; } = string.Empty;
    }
}
