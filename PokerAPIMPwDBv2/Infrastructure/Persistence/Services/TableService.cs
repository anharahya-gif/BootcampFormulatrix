using PokerAPIMPwDB.Infrastructure.Persistence;
using PokerAPIMPwDB.Infrastructure.Persistence.Entities;
using PokerAPIMPwDB.Infrastructure.Persistence.Repositories;
using PokerAPIMPwDB.Common.Results;
using PokerAPIMPwDB.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TableService : ITableService
{
    private readonly ITableRepository _tableRepo;

    public TableService(ITableRepository tableRepo)
    {
        _tableRepo = tableRepo;
    }

    public async Task<ServiceResult<List<Table>>> GetAllTablesAsync()
    {
        var tables = await _tableRepo.GetActiveTablesAsync();
        return ServiceResult<List<Table>>.Success(tables.ToList());
    }

    public async Task<ServiceResult<Table>> GetTableByIdAsync(Guid tableId)
    {
        var table = await _tableRepo.GetTableWithSeatsAsync(tableId);

        if (table == null)
            return ServiceResult<Table>.Fail("Table not found");

        return ServiceResult<Table>.Success(table);
    }

    public async Task<ServiceResult<Table>> CreateTableAsync(Table table, int seatCount = 6)
    {
        table.Id = Guid.NewGuid();
        table.CreatedAt = DateTime.UtcNow;
        // Status logic might be handled by Engine, but for DB entity:
        // table.Status handle via FluentAPI default or here

        table.PlayerSeats = new List<PlayerSeat>();
        for (int i = 0; i < seatCount; i++)
        {
            table.PlayerSeats.Add(new PlayerSeat
            {
                SeatNumber = i,
                PlayerId = null
            });
        }

        await _tableRepo.AddAsync(table);
        await _tableRepo.SaveChangesAsync();

        return ServiceResult<Table>.Success(table, "Table created with seats");
    }

    public async Task<ServiceResult> UpdateTableAsync(Guid tableId, Table updatedTable)
    {
        var table = await _tableRepo.GetByIdAsync(tableId);
        if (table == null) return ServiceResult.Fail("Table not found");

        table.Name = updatedTable.Name;
        table.MaxPlayers = updatedTable.MaxPlayers;
        table.MinBuyIn = updatedTable.MinBuyIn;
        table.MaxBuyIn = updatedTable.MaxBuyIn;
        table.SmallBlind = updatedTable.SmallBlind;
        table.BigBlind = updatedTable.BigBlind;

        _tableRepo.Update(table);
        await _tableRepo.SaveChangesAsync();
        return ServiceResult.Success("Table updated successfully");
    }

    public async Task<ServiceResult> DeleteTableAsync(Guid tableId)
    {
        var table = await _tableRepo.GetByIdAsync(tableId);
        if (table == null) return ServiceResult.Fail("Table not found");

        _tableRepo.Delete(table);
        await _tableRepo.SaveChangesAsync();
        return ServiceResult.Success("Table deleted successfully");
    }
    
    public async Task<ServiceResult> SoftDeleteTableAsync(Guid tableId)
    {
        var table = await _tableRepo.GetByIdAsync(tableId);
        if (table == null) return ServiceResult.Fail("Table not found");

        table.isDeleted = true;
        _tableRepo.Update(table);
        await _tableRepo.SaveChangesAsync();
        return ServiceResult.Success("Table soft deleted successfully");
    }
}
