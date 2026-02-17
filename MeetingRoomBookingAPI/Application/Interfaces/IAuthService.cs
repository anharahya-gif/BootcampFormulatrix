using System.Threading.Tasks;
using MeetingRoomBookingAPI.Application.Common;
using MeetingRoomBookingAPI.Application.DTOs.Auth;
using MeetingRoomBookingAPI.Domain.Entities;
using System.Collections.Generic;
using System;

namespace MeetingRoomBookingAPI.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto registerDto);
        Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto loginDto);
    }

    public interface ITokenService
    {
        string CreateToken(ApplicationUser user, IList<string> roles);
    }
}
