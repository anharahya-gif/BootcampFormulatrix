using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PokerAPIMPwDB.DTO.User;
using PokerAPIMPwDB.Infrastructure.Persistence.Entities;

namespace PokerAPIMPwDB.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        // ==============================
        // GET: api/user/profile
        // ==============================
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            var result = await _userService.GetUserByIdAsync(userId);

            if (!result.IsSuccess)
                return NotFound(result.ErrorMessage);

            var user = result.Value;
            return Ok(new UserInfoDto
            {
                Id = user.Id,
                Username = user!.UserName!,
                Balance = user.Balance,
                CreatedAt = user.CreatedAt
            });
        }

        // ==============================
        // GET: api/user
        // ==============================
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var result = await _userService.GetAllUsersAsync();

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            var dtos = result.Value!.Select(u => new UserInfoDto
            {
                Id = u.Id,
                Username = u.UserName!,
                Balance = u.Balance,
                CreatedAt = u.CreatedAt
            });

            return Ok(dtos);
        }

        // ==============================
        // GET: api/user/{id}
        // ==============================
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var result = await _userService.GetUserByIdAsync(id);

            if (!result.IsSuccess)
                return NotFound(result.ErrorMessage);

            var user = result.Value;

            var dto = new UserInfoDto
            {
                Id = user.Id,
                Username = user!.UserName!,
                Balance = user.Balance,
                CreatedAt = user.CreatedAt
            };

            return Ok(dto);
        }

        // ==============================
        // POST: api/user
        // ==============================
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest("Username is required");

            if (string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Password is required");

            var user = new User
            {
                UserName = request.Username,
                Balance = request.Balance
            };

            var result = await _userService.CreateUserAsync(user);

            if (!result.IsSuccess)
                return BadRequest(result.ErrorMessage);

            var createdUser = result.Value;

            var response = new UserInfoDto
            {
                Id = createdUser.Id,
                Username = createdUser!.UserName!,
                Balance = createdUser.Balance,
                CreatedAt = createdUser.CreatedAt
            };

            return CreatedAtAction(nameof(GetUser),
                new { id = createdUser.Id },
                response);
        }

        // ==============================
        // PUT: api/user/{id}
        // ==============================
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto request)
        {
            var result = await _userService.UpdateUserAsync(id, request);

            if (!result.IsSuccess)
                return NotFound(result.ErrorMessage);

            return Ok("User updated successfully");
        }

        // ==============================
        // DELETE (Soft Delete)
        // ==============================
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> SoftDeleteUser(Guid id)
        {
            var result = await _userService.SoftDeleteUserAsync(id);

            if (!result.IsSuccess)
                return NotFound(result.ErrorMessage);

            return NoContent();
        }

        // ==============================
        // POST: api/user/deposit
        // ==============================
        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositDto request)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
            if (userId == Guid.Empty) return Unauthorized();

            var result = await _userService.DepositAsync(userId, request.Amount);
            if (!result.IsSuccess) return BadRequest(result.ErrorMessage);

            return Ok(result.Message);
        }
    }
}
