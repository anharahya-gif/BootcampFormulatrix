namespace PokerAPIMPwDB.Infrastructure.Persistence.Entities
{
    public class PlayerSeat
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int SeatNumber { get; set; }

        // ===== Table =====
        public Guid TableId { get; set; }
        public Table Table { get; set; } = null!;

        // ===== Player (OPTIONAL) =====
        public Guid? PlayerId { get; set; }
        public Player? Player { get; set; }
    }
}
