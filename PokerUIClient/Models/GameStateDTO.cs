namespace PokerUIClient.Models
{


    public class GameStateDTO
    {
        public List<PlayerDTO>? Players { get; set; }
        public int Pot { get; set; }
        public string? gameState { get; set; }
        public string? Phase { get; set; } // PreFlop, Flop, Turn, River
        public string? CurrentPlayer { get; set; }
        public int CurrentBet { get; set; }

        public List<string> CommunityCards { get; set; } = new List<string>();
        public ShowdownDTO? Showdown { get; set; }
        public Dictionary<int, PlayerDTO> PlayerSeatMap { get; set; } = new();
        public List<int> LastWinnerSeatIndexes { get; set; } = new();
    }
}