using System.Security.Cryptography.X509Certificates;
using PokerAPIMPwDB.Domain.Enums;
using PokerAPIMPwDB.Domain.Interfaces;
namespace PokerAPIMPwDB.Infrastructure.Persistence.Entities
{


    public class Table
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public int MaxPlayers { get; set; }
        public int MinBuyIn { get; set; }
        public int MaxBuyIn { get; set; }
        public int SmallBlind { get; set; }
        public int BigBlind { get; set; }
        public TableState Status { get; set; } = TableState.Waiting;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool isDeleted {get;set;}=false;

        // Relasi ke PlayerSeat
        public ICollection<PlayerSeat> PlayerSeats { get; set; } = new List<PlayerSeat>();
    }

}