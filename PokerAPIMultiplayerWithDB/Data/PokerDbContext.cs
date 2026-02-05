using Microsoft.EntityFrameworkCore;
using PokerAPIMultiplayerWithDB.Models;

namespace PokerAPIMultiplayerWithDB.Data
{
    public class PokerDbContext : DbContext
    {
        public PokerDbContext(DbContextOptions<PokerDbContext> options) : base(options) { }

        public DbSet<Player> Players { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<PlayerAtTable> PlayerAtTables { get; set; }
        public DbSet<GameLog> GameLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Player>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
            });

            modelBuilder.Entity<Table>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MaxPlayers).HasDefaultValue(10);
            });

            modelBuilder.Entity<PlayerAtTable>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(p => p.Player).WithMany(p => p.PlayerAtTables).HasForeignKey(p => p.PlayerId);
                entity.HasOne(p => p.Table).WithMany(t => t.PlayerAtTables).HasForeignKey(p => p.TableId);
                entity.HasIndex(p => new { p.TableId, p.SeatNumber }).IsUnique();
            });

            modelBuilder.Entity<GameLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasOne(g => g.Table).WithMany(t => t.GameLogs).HasForeignKey(g => g.TableId);
            });
        }
    }
}
