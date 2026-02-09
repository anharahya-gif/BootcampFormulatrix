using System.Collections.Generic;

namespace PokerAPI.DTOs
{
    public class GameStateDto
    {
        public string GameState { get; set; } = string.Empty;
        public string Phase { get; set; } = string.Empty;
        public string? CurrentPlayer { get; set; }
        public int CurrentBet { get; set; }
        public int Pot { get; set; }
        public List<string> CommunityCards { get; set; } = new();
        public List<PlayerPublicStateDto> Players { get; set; } = new();
        public ShowdownDto? Showdown { get; set; }
    }
}
