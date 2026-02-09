using System.Collections.Generic;

namespace PokerAPI.DTOs
{
    public class ShowdownDto
    {
        public List<string> Winners { get; set; } = new();
        public string HandRank { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
