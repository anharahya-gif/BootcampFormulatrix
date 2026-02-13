using PokerAPIMPwDB.Infrastructure.Persistence.Entities;
using PokerAPIMPwDB.Common.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ITableService
{
    Task<ServiceResult<List<Table>>> GetAllTablesAsync();
    Task<ServiceResult<Table>> GetTableByIdAsync(Guid tableId);
    Task<ServiceResult<Table>> CreateTableAsync(Table table,int seatCount);
    Task<ServiceResult> UpdateTableAsync(Guid tableId, Table updatedTable);
    Task<ServiceResult> DeleteTableAsync(Guid tableId);
    Task<ServiceResult> SoftDeleteTableAsync(Guid tableId);
}
