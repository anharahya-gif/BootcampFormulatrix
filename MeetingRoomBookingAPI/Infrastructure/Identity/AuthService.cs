using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeetingRoomBookingAPI.Application.Common;
using MeetingRoomBookingAPI.Application.DTOs.Auth;
using MeetingRoomBookingAPI.Application.Interfaces;
using MeetingRoomBookingAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace MeetingRoomBookingAPI.Infrastructure.Identity
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ITokenService _tokenService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
        }

        public async Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            var user = new ApplicationUser
            {
                UserName = registerDto.Email,
                Email = registerDto.Email,
                Profile = new UserProfile
                {
                    FullName = registerDto.FullName,
                    Department = registerDto.Department,
                    PhoneNumber = registerDto.PhoneNumber
                }
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return ServiceResult<AuthResponseDto>.FailureResult(errors);
            }

            if (!await _roleManager.RoleExistsAsync("User"))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>("User"));
            }
            await _userManager.AddToRoleAsync(user, "User");

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);

            return ServiceResult<AuthResponseDto>.SuccessResult(new AuthResponseDto
            {
                Success = true,
                Token = token,
                UserName = user.UserName,
                Email = user.Email,
                Role = roles.Contains("Admin") ? "Admin" : roles.FirstOrDefault()
            });
        }

        public async Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);

            if (user == null)
            {
                return ServiceResult<AuthResponseDto>.FailureResult("Invalid credentials", 401);
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (!result.Succeeded)
            {
                return ServiceResult<AuthResponseDto>.FailureResult("Invalid credentials", 401);
            }

            var roles = await _userManager.GetRolesAsync(user);
            var token = _tokenService.CreateToken(user, roles);

            return ServiceResult<AuthResponseDto>.SuccessResult(new AuthResponseDto
            {
                Success = true,
                Token = token,
                UserName = user.UserName,
                Email = user.Email,
                Role = roles.Contains("Admin") ? "Admin" : roles.FirstOrDefault()
            });
        }
    }
}
