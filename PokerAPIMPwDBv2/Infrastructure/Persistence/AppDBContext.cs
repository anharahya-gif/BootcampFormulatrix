using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PokerAPIMPwDB.Infrastructure.Persistence.Entities;

namespace PokerAPIMPwDB.Infrastructure.Persistence
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Table> Tables { get; set; } = null!;
        public DbSet<Player> Players { get; set; } = null!;
        public DbSet<PlayerSeat> PlayerSeats { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table Configuration
            modelBuilder.Entity<Table>(entity =>
            {
                entity.ToTable("Tables");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.MinBuyIn).IsRequired();
                entity.Property(e => e.MaxBuyIn).IsRequired();
                entity.Property(e => e.SmallBlind).IsRequired();
                entity.Property(e => e.BigBlind).IsRequired();
            });

            // PlayerSeat Configuration
            modelBuilder.Entity<PlayerSeat>(entity =>
            {
                entity.ToTable("PlayerSeats");
                entity.HasKey(e => e.Id);
                
                entity.HasOne(d => d.Table)
                    .WithMany(p => p.PlayerSeats)
                    .HasForeignKey(d => d.TableId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Player)
                    .WithOne(p => p.PlayerSeat)
                    .HasForeignKey<PlayerSeat>(d => d.PlayerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Player Configuration
            modelBuilder.Entity<Player>(entity =>
            {
                entity.ToTable("Players");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(50);
                
                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // User Configuration (extends IdentityUser)
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Balance).IsRequired().HasDefaultValue(0);
                entity.Property(e => e.isDeleted).HasDefaultValue(false);
            });
        }
    }
}
