namespace PokerBetterUI.Models
{

    public class PlayerDTO
    {
        public string Name { get; set; } = string.Empty;
        public int ChipStack { get; set; }
        public int CurrentBet { get; set; }
        public bool IsFolded { get; set; }

        public List<string> Hand { get; set; } = new List<string>();

        // =========================
        // NEW: State property
        // =========================
        public string State { get; set; } = string.Empty;
        public int SeatIndex { get; set; }

        public List<int> LastWinnerSeatIndexes { get; set; } = new();

    }
}
