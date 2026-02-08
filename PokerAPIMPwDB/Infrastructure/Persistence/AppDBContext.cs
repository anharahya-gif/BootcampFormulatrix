using Microsoft.EntityFrameworkCore;
using PokerAPIMPwDB.Infrastructure.Persistence.Entities;

namespace PokerAPIMPwDB.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Table> Tables { get; set; } = null!;
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<PlayerSeat> PlayerSeats { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Optional: konfigurasi relasi 1:n Table → PlayerSeats
            modelBuilder.Entity<Table>()
                .HasMany(t => t.PlayerSeats)
                .WithOne(ps => ps.Table)
                .HasForeignKey(ps => ps.TableId)
                .OnDelete(DeleteBehavior.Cascade);

            // Optional: relasi 1:1 Player → PlayerSeat
            modelBuilder.Entity<Player>()
                .HasOne(p => p.PlayerSeat)
                .WithOne(ps => ps.Player)
                .HasForeignKey<PlayerSeat>(ps => ps.PlayerId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
