// using Microsoft.AspNetCore.Mvc;
// using CobaWebAPI.DTOs;
// using CobaWebAPI.Services;

// namespace CobaWebAPI.Controllers
// {
//     [ApiController]
//     [Route("api/auth")]
//     public class AuthController : ControllerBase
//     {
//         private readonly IAuthService _auth;

//         public AuthController(IAuthService auth)
//         {
//             _auth = auth;
//         }

//         [HttpPost("register")]
//         public IActionResult Register(RegisterDto dto)
//         {
//             var user = _auth.Register(dto.Username, dto.Password);
//             return Ok(new { message = "Register success" });
//         }

//         [HttpPost("login")]
//         public IActionResult Login(LoginDto dto)
//         {
//             var user = _auth.Login(dto.Username, dto.Password);
//             if (user == null)
//                 return Unauthorized("Username atau password salah");

//             var token = _auth.GenerateToken(user);
//             return Ok(new { token });
//         }
//     }
// }
using CobaWebAPI.DTOs.Auth;
using CobaWebAPI.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CobaWebAPI.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto dto)
        {
            await _authService.RegisterAsync(dto);
            return Ok(new { message = "Register success" });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }


    }
}
