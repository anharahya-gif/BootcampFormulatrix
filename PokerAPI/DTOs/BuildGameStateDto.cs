using System.Collections.Generic;

namespace PokerAPI.DTOs
{
    // DTO utama untuk Game State
    public class GameStateDto
    {
        public string Phase { get; set; } = string.Empty;
        public string? CurrentPlayer { get; set; }
        public int CurrentBet { get; set; }
        public int Pot { get; set; }
        public List<string> CommunityCards { get; set; } = new();
        public List<PlayerPublicStateDto> Players { get; set; } = new();
        public ShowdownDto? Showdown { get; set; }
    }

    // DTO untuk pemain (public state)
    public class PlayerPublicStateDto
    {
        public string Name { get; set; } = string.Empty;
        public int ChipStack { get; set; }
        public int CurrentBet { get; set; }
        public bool IsFolded { get; set; }
        public int SeatIndex { get; set; }
        public string State { get; set; } = string.Empty;
        public List<string> Hand { get; set; } = new();
    }

    // DTO untuk showdown terakhir
    public class ShowdownDto
    {
        public List<string> Winners { get; set; } = new();
        public string HandRank { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
