using System.Collections.Concurrent;
using PokerMultiplayerAPI.Domain.Entities;
using PokerMultiplayerAPI.Domain.Interfaces;

namespace PokerMultiplayerAPI.Infrastructure.Persistence;

public class InMemoryTableRepository : ITableRepository
{
    // Thread-safe dictionary to store tables
    private readonly ConcurrentDictionary<Guid, Table> _tables = new();

    public InMemoryTableRepository()
    {
        // Organize a default table for testing
        var defaultTable = new Table { Name = "Main Floor Table" };
        _tables.TryAdd(defaultTable.Id, defaultTable);
    }

    public Table GetTable(Guid tableId)
    {
        _tables.TryGetValue(tableId, out var table);
        return table;
    }

    public IEnumerable<Table> GetAllTables()
    {
        return _tables.Values;
    }

    public void UpdateTable(Table table)
    {
        // In-memory reference is already updated, but explicit update method is good for future DB swap
        _tables.AddOrUpdate(table.Id, table, (key, oldValue) => table);
    }

    public Table CreateTable(string name)
    {
        var table = new Table { Name = name };
        _tables.TryAdd(table.Id, table);
        return table;
    }
}
