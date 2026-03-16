namespace PokerAPIMPwDB.DTO.Table
{
    public class SeatStateDto
    {
        public int SeatIndex { get; set; }
        public bool IsOccupied { get; set; }

        public Guid? PlayerId { get; set; }
        public string? PlayerName { get; set; }

        public int Chips { get; set; }
        public bool IsFolded { get; set; }
        public bool IsAllIn { get; set; }
    }
}
