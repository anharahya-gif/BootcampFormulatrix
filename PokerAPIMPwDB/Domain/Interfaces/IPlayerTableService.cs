using PokerAPIMPwDB.Infrastructure.Persistence.Entities;
using PokerAPIMPwDB.Common.Results;
using System;
using System.Threading.Tasks;

public interface IPlayerServiceTable
{
    Task<ServiceResult<Player>> JoinTableAsync(Guid tableId, Guid userId, int buyInAmount, int seatNumber);
    Task<ServiceResult<bool>> LeaveTableAsync(Guid tableId, Guid playerId);
}