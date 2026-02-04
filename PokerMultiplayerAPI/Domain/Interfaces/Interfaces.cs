using PokerMultiplayerAPI.Domain.Entities;
using PokerMultiplayerAPI.Domain.Enums;
using PokerMultiplayerAPI.Shared.DTOs;

namespace PokerMultiplayerAPI.Domain.Interfaces;

public interface ITableRepository
{
    Table GetTable(Guid tableId);
    IEnumerable<Table> GetAllTables();
    void UpdateTable(Table table);
    Table CreateTable(string name);
}

public interface IGameNotifier
{
    Task NotifyGameStateChanged(Guid tableId, TableStateDto gameState);
    Task NotifyPlayerAction(Guid tableId, string playerName, string action, decimal amount);
    Task NotifyError(string connectionId, string errorMessage);
    Task NotifyHandResult(Guid tableId, string resultMessage); // Placeholder for now
    Task NotifyGameStateChangedForPlayer(Guid tableId, Guid playerId, TableStateDto state);
}

public interface IGameService
{
    Task<Table> JoinTableAsync(Guid tableId, Guid playerId, string playerName, decimal chips);
    Task LeaveTableAsync(Guid tableId, Guid playerId);
    Task<bool> PlayerActionAsync(Guid tableId, Guid playerId, TurnAction action, decimal amount);
    Task StartGameAsync(Guid tableId);
}
