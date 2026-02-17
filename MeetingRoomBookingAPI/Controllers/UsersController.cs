using System;
using System.Threading.Tasks;
using MeetingRoomBookingAPI.Application.Interfaces;
using MeetingRoomBookingAPI.Application.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MeetingRoomBookingAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllUsersAsync();
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(UserCreateDto dto)
        {
            var result = await _userService.CreateUserAsync(dto);
            if (!result.Success) return BadRequest(result);
            return CreatedAtAction(nameof(GetAll), null, result);
        }

        [HttpPost("{id}/role")]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] string roleName)
        {
            var result = await _userService.AssignRoleAsync(id, roleName);
            if (!result.Success) return result.StatusCode == 404 ? NotFound(result) : BadRequest(result);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success) return result.StatusCode == 404 ? NotFound(result) : BadRequest(result);
            return Ok(result);
        }
    }
}
