using System.ComponentModel.DataAnnotations;

namespace PokerAPIMultiplayerWithDB.Models
{
    public enum TableStatus { Open, InProgress, Closed }

    public class Table
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string TableName { get; set; }

        public TableStatus Status { get; set; } = TableStatus.Open;

        public long MinBuyIn { get; set; }
        public long MaxBuyIn { get; set; }

        public int MaxPlayers { get; set; } = 10;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<PlayerAtTable> PlayerAtTables { get; set; }
        public ICollection<GameLog> GameLogs { get; set; }
    }
}
