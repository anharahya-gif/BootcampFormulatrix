using System;
using System.Security.Claims;
using System.Threading.Tasks;
using MeetingRoomBookingAPI.Application.DTOs.User;
using MeetingRoomBookingAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetingRoomBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;

        public ProfileController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
            {
                return Unauthorized();
            }

            var result = await _userService.GetUserByIdAsync(userId);
            if (!result.Success) return result.StatusCode == 404 ? NotFound(result) : BadRequest(result);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile(UserUpdateDto dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid userId))
            {
                return Unauthorized();
            }

            var result = await _userService.UpdateProfileAsync(userId, dto);
            if (!result.Success) return result.StatusCode == 404 ? NotFound(result) : BadRequest(result);
            return Ok(result);
        }
    }
}
