using PokerAPIMPwDB.Infrastructure.Persistence;
using PokerAPIMPwDB.Infrastructure.Persistence.Entities;
using PokerAPIMPwDB.Common.Results;
using PokerAPIMPwDB.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TableService : ITableService
{
    private readonly AppDbContext _db;

    public TableService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ServiceResult<List<Table>>> GetAllTablesAsync()
    {
        var tables = await _db.Tables.Include(t => t.PlayerSeats).ToListAsync();
        return ServiceResult<List<Table>>.Success(tables);
    }

    public async Task<ServiceResult<Table>> GetTableByIdAsync(Guid tableId)
    {
        var table = await _db.Tables.Include(t => t.PlayerSeats)
                                    .FirstOrDefaultAsync(t => t.Id == tableId);

        if (table == null)
            return ServiceResult<Table>.Fail("Table not found");

        return ServiceResult<Table>.Success(table);
    }

public async Task<ServiceResult<Table>> CreateTableAsync(Table table, int seatCount = 6)
{
    table.Id = Guid.NewGuid();
    table.CreatedAt = DateTime.UtcNow;
    table.Status = TableState.Waiting;

    // Generate seat otomatis
    table.PlayerSeats = new List<PlayerSeat>();
    for (int i = 0; i < seatCount; i++)
    {
        table.PlayerSeats.Add(new PlayerSeat
        {
            SeatNumber = i,
            Player = null // kosong dulu
        });
    }

    _db.Tables.Add(table);
    await _db.SaveChangesAsync();

    return ServiceResult<Table>.Success(table, "Table created with seats");
}


    public async Task<ServiceResult> UpdateTableAsync(Guid tableId, Table updatedTable)
    {
        var table = await _db.Tables.FindAsync(tableId);
        if (table == null) return ServiceResult.Fail("Table not found");

        table.Name = updatedTable.Name;
        table.MaxPlayers = updatedTable.MaxPlayers;

        await _db.SaveChangesAsync();
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteTableAsync(Guid tableId)
    {
        var table = await _db.Tables.FindAsync(tableId);
        if (table == null) return ServiceResult.Fail("Table not found");

        _db.Tables.Remove(table);
        await _db.SaveChangesAsync();
        return ServiceResult.Success();
    }
}
