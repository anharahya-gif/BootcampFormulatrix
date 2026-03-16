using Microsoft.EntityFrameworkCore;
using CobaWebAPI.Entities;

namespace CobaWebAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // sementara kosong dulu
        // nanti kita tambahin DbSet<User>, DbSet<Product>, dll
        public DbSet<User> Users => Set<User>();
    }
}
