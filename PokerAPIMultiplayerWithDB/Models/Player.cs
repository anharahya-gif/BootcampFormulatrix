using System.ComponentModel.DataAnnotations;

namespace PokerAPIMultiplayerWithDB.Models
{
    public class Player
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public long ChipBalance { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<PlayerAtTable> PlayerAtTables { get; set; }
    }
}
