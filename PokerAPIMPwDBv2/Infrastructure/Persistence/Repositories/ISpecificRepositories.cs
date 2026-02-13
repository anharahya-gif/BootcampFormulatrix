using PokerAPIMPwDB.Infrastructure.Persistence.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PokerAPIMPwDB.Infrastructure.Persistence.Repositories
{
    public interface ITableRepository : IRepository<Table>
    {
        Task<Table?> GetTableWithSeatsAsync(Guid tableId);
        Task<IEnumerable<Table>> GetActiveTablesAsync();
    }

    public interface IPlayerRepository : IRepository<Player>
    {
        Task<Player?> GetPlayerWithSeatAsync(Guid userId);
    }
}
