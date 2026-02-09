using System.Collections.Generic;

namespace PokerAPI.DTOs
{
    public class ShowdownResultDto
    {
        public List<string> Winners { get; set; } = new();
        public List<ShowdownPlayerDto> Players { get; set; } = new();
        public List<string> CommunityCards { get; set; } = new();
        public string HandRank { get; set; } = string.Empty;
        public int Pot { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
