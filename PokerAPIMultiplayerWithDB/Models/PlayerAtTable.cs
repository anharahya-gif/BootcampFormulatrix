using System.ComponentModel.DataAnnotations;

namespace PokerAPIMultiplayerWithDB.Models
{
    public class PlayerAtTable
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }
        public Player Player { get; set; }

        public int TableId { get; set; }
        public Table Table { get; set; }

        public int SeatNumber { get; set; }

        public long ChipDeposit { get; set; }

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LeftAt { get; set; }

        // Player state during hand (Active, Folded) - simplified
        public bool HasFolded { get; set; } = false;
    }
}
