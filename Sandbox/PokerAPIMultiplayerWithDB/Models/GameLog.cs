using System.ComponentModel.DataAnnotations;

namespace PokerAPIMultiplayerWithDB.Models
{
    public class GameLog
    {
        public int Id { get; set; }

        public int TableId { get; set; }
        public Table Table { get; set; }

        public int? RoundNumber { get; set; }

        [MaxLength(200)]
        public string Action { get; set; }

        // JSON-formatted details about the round
        public string DetailsJson { get; set; }

        public int? WinnerPlayerId { get; set; }

        public long? TotalPot { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
