using PokerAPIMPwDB.Infrastructure.Persistence.Entities;
using PokerAPIMPwDB.Common.Results;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PokerAPIMPwDB.DTO.User;

public interface IUserService
{
    Task<ServiceResult<List<User>>> GetAllUsersAsync();
    Task<ServiceResult<User>> GetUserByIdAsync(Guid userId);
    Task<ServiceResult<User>> CreateUserAsync(User user);
    Task<ServiceResult<User>> UpdateUserAsync(Guid id, UpdateUserDto request);
    // Task<ServiceResult> DeleteUserAsync(Guid userId);
    Task<ServiceResult> SoftDeleteUserAsync(Guid userId);
    Task<ServiceResult> DepositAsync(Guid userId, int amount);
}
