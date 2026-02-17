using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeetingRoomBookingAPI.Application.Common;
using MeetingRoomBookingAPI.Application.DTOs.User;

namespace MeetingRoomBookingAPI.Application.Interfaces
{
    public interface IUserService
    {
        Task<ServiceResult<IEnumerable<UserReadDto>>> GetAllUsersAsync();
        Task<ServiceResult<UserReadDto>> GetUserByIdAsync(Guid userId);
        Task<ServiceResult<bool>> AssignRoleAsync(Guid userId, string roleName);
        Task<ServiceResult<UserReadDto>> CreateUserAsync(UserCreateDto dto);
        Task<ServiceResult<UserReadDto>> UpdateProfileAsync(Guid userId, UserUpdateDto dto);
        Task<ServiceResult<bool>> DeleteUserAsync(Guid userId);
    }
}
