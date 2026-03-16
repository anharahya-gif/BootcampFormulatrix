using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PokerAPIMultiplayerWithDB.Data;
using PokerAPIMultiplayerWithDB.Models;
using PokerAPIMultiplayerWithDB.Services;

namespace PokerAPIMultiplayerWithDB.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly PokerDbContext _db;
        private readonly IJwtTokenService _jwt;

        public AuthController(PokerDbContext db, IJwtTokenService jwt)
        {
            _db = db;
            _jwt = jwt;
        }

        public class RegisterRequest { public string Username { get; set; } public string Password { get; set; } }
        public class AuthResponse { public string Token { get; set; } public int ExpiresInMinutes { get; set; } }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || req.Username.Length < 3) return BadRequest("Username too short");
            if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 8) return BadRequest("Password too short");

            var exists = await _db.Players.AnyAsync(p => p.Username == req.Username);
            if (exists) return Conflict("Username already exists");

            var player = new Player
            {
                Username = req.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                ChipBalance = 10000 // initial chips for testing
            };

            _db.Players.Add(player);
            await _db.SaveChangesAsync();

            return Ok(new { message = "registered" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] RegisterRequest req)
        {
            var player = await _db.Players.SingleOrDefaultAsync(p => p.Username == req.Username);
            if (player == null) return Unauthorized("Invalid credentials");

            if (!BCrypt.Net.BCrypt.Verify(req.Password, player.PasswordHash)) return Unauthorized("Invalid credentials");

            var token = _jwt.GenerateToken(player.Id, 60);
            return Ok(new AuthResponse { Token = token, ExpiresInMinutes = 60 });
        }
    }
}
