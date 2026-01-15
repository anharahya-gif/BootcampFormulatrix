using BCrypt.Net;
using CobaWebAPI.DTOs.Users;
using CobaWebAPI.Entities;
using CobaWebAPI.Data;
using CobaWebAPI.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CobaWebAPI.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllAsync()
        {
            return await _context.Users
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<UserResponseDto> GetByIdAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id)
                ?? throw new Exception("User not found");

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task CreateAsync(CreateUserDto dto)
        {
            var exists = await _context.Users
                .AnyAsync(u => u.Username == dto.Username);

            if (exists)
                throw new Exception("Username already exists");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = dto.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid id, UpdateUserDto dto)
        {
            var user = await _context.Users.FindAsync(id)
                ?? throw new Exception("User not found");

            user.Username = dto.Username;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var user = await _context.Users.FindAsync(id)
                ?? throw new Exception("User not found");

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }
}
