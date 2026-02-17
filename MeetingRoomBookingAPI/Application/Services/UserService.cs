using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MeetingRoomBookingAPI.Application.Common;
using MeetingRoomBookingAPI.Application.DTOs.User;
using MeetingRoomBookingAPI.Application.Interfaces;
using MeetingRoomBookingAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MeetingRoomBookingAPI.Application.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IMapper _mapper;

        public UserService(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<ServiceResult<IEnumerable<UserReadDto>>> GetAllUsersAsync()
        {
            var users = await _userManager.Users
                .Include(u => u.Profile)
                .ToListAsync();
            
            var userDtos = new List<UserReadDto>();
            foreach (var user in users)
            {
                var dto = _mapper.Map<UserReadDto>(user);
                var roles = await _userManager.GetRolesAsync(user);
                dto.Role = roles.Contains("Admin") ? "Admin" : roles.FirstOrDefault() ?? "User";
                userDtos.Add(dto);
            }
            
            return ServiceResult<IEnumerable<UserReadDto>>.SuccessResult(userDtos);
        }

        public async Task<ServiceResult<UserReadDto>> GetUserByIdAsync(Guid userId)
        {
            var user = await _userManager.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return ServiceResult<UserReadDto>.FailureResult("User not found", 404);

            var dto = _mapper.Map<UserReadDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            dto.Role = roles.Contains("Admin") ? "Admin" : roles.FirstOrDefault() ?? "User";

            return ServiceResult<UserReadDto>.SuccessResult(dto);
        }

        public async Task<ServiceResult<bool>> AssignRoleAsync(Guid userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null) return ServiceResult<bool>.FailureResult("User not found", 404);

            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }

            // Remove existing roles first to ensure we only have the target role (as requested: "changing role")
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);

            var result = await _userManager.AddToRoleAsync(user, roleName);
            if (!result.Succeeded)
            {
                return ServiceResult<bool>.FailureResult(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return ServiceResult<bool>.SuccessResult(true);
        }

        public async Task<ServiceResult<UserReadDto>> CreateUserAsync(UserCreateDto dto)
        {
            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null) return ServiceResult<UserReadDto>.FailureResult("Email already exists");

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                Profile = new UserProfile
                {
                    FullName = dto.FullName,
                    Department = dto.Department,
                    PhoneNumber = dto.PhoneNumber
                }
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return ServiceResult<UserReadDto>.FailureResult(string.Join(", ", result.Errors.Select(e => e.Description)));

            if (!await _roleManager.RoleExistsAsync(dto.Role))
                await _roleManager.CreateAsync(new IdentityRole<Guid>(dto.Role));

            await _userManager.AddToRoleAsync(user, dto.Role);

            var readDto = _mapper.Map<UserReadDto>(user);
            readDto.Role = dto.Role;
            return ServiceResult<UserReadDto>.SuccessResult(readDto, 201);
        }

        public async Task<ServiceResult<UserReadDto>> UpdateProfileAsync(Guid userId, UserUpdateDto dto)
        {
            var user = await _userManager.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return ServiceResult<UserReadDto>.FailureResult("User not found", 404);

            if (user.Profile == null)
            {
                user.Profile = new UserProfile { FullName = dto.FullName ?? user.UserName! };
            }

            if (dto.FullName != null) user.Profile.FullName = dto.FullName;
            if (dto.Department != null) user.Profile.Department = dto.Department;
            if (dto.PhoneNumber != null) user.Profile.PhoneNumber = dto.PhoneNumber;
            if (dto.AvatarUrl != null) user.Profile.AvatarUrl = dto.AvatarUrl;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return ServiceResult<UserReadDto>.FailureResult(string.Join(", ", result.Errors.Select(e => e.Description)));

            var readDto = _mapper.Map<UserReadDto>(user);
            var roles = await _userManager.GetRolesAsync(user);
            readDto.Role = roles.Contains("Admin") ? "Admin" : roles.FirstOrDefault() ?? "User";

            return ServiceResult<UserReadDto>.SuccessResult(readDto);
        }

        public async Task<ServiceResult<bool>> DeleteUserAsync(Guid userId)
        {
            var user = await _userManager.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return ServiceResult<bool>.FailureResult("User not found", 404);

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;

            if (user.Profile != null)
            {
                user.Profile.IsDeleted = true;
                user.Profile.DeletedAt = DateTime.UtcNow;
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return ServiceResult<bool>.FailureResult(string.Join(", ", result.Errors.Select(e => e.Description)));

            return ServiceResult<bool>.SuccessResult(true);
        }
    }
}
