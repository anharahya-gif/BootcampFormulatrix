using Microsoft.EntityFrameworkCore;
using PokerAPIMPwDB.Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PokerAPIMPwDB.Infrastructure.Persistence.Repositories
{
    public class TableRepository : Repository<Table>, ITableRepository
    {
        public TableRepository(AppDbContext context) : base(context) { }

        public async Task<Table?> GetTableWithSeatsAsync(Guid tableId)
        {
            return await _context.Tables
                .Include(t => t.PlayerSeats)
                .ThenInclude(ps => ps.Player)
                .FirstOrDefaultAsync(t => t.Id == tableId);
        }

        public async Task<IEnumerable<Table>> GetActiveTablesAsync()
        {
            return await _context.Tables
                .Include(t => t.PlayerSeats)
                .Where(t => !t.isDeleted)
                .ToListAsync();
        }
    }

    public class PlayerRepository : Repository<Player>, IPlayerRepository
    {
        public PlayerRepository(AppDbContext context) : base(context) { }

        public async Task<Player?> GetPlayerWithSeatAsync(Guid userId)
        {
            return await _context.Players
                .Include(p => p.PlayerSeat)
                .FirstOrDefaultAsync(p => p.UserId == userId);
        }
    }
}
