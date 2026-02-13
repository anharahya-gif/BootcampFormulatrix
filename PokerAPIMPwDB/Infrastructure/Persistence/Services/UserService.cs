using PokerAPIMPwDB.Infrastructure.Persistence;
using PokerAPIMPwDB.Infrastructure.Persistence.Entities;
using PokerAPIMPwDB.Common.Results;
using Microsoft.EntityFrameworkCore;
using PokerAPIMPwDB.DTO.User;

public class UserService : IUserService
{
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    // ==============================
    // GET ALL
    // ==============================
    public async Task<ServiceResult<List<User>>> GetAllUsersAsync()
    {
        var users = await _db.Users
            .Where(u => !u.isDeleted)
            .ToListAsync();

        return ServiceResult<List<User>>.Success(users);
    }

    // ==============================
    // GET BY ID
    // ==============================
    public async Task<ServiceResult<User>> GetUserByIdAsync(Guid userId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.isDeleted);

        if (user == null)
            return ServiceResult<User>.Fail("User not found");

        return ServiceResult<User>.Success(user);
    }

    // ==============================
    // CREATE
    // ==============================
    public async Task<ServiceResult<User>> CreateUserAsync(User user)
    {
        if (string.IsNullOrWhiteSpace(user.UserName))
            return ServiceResult<User>.Fail("Username is required");

        user.CreatedAt = DateTime.UtcNow;
        user.isDeleted = false;

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return ServiceResult<User>.Success(user, "User created successfully");
    }

    // ==============================
    // UPDATE
    // ==============================
    public async Task<ServiceResult<User>> UpdateUserAsync(Guid id, UpdateUserDto request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && !u.isDeleted);

        if (user == null)
            return ServiceResult<User>.Fail("User not found");

        // Update hanya field yang dikirim
        if (!string.IsNullOrWhiteSpace(request.Username))
            user.UserName = request.Username;

        if (request.Balance.HasValue)
            user.Balance = request.Balance.Value;

        await _db.SaveChangesAsync();

        return ServiceResult<User>.Success(user, "User updated successfully");
    }

    // ==============================
    // SOFT DELETE
    // ==============================
    public async Task<ServiceResult> SoftDeleteUserAsync(Guid userId)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Id == userId && !u.isDeleted);

        if (user == null)
            return ServiceResult.Fail("User not found");

        user.isDeleted = true;

        await _db.SaveChangesAsync();
        return ServiceResult.Success("User deleted successfully");
    }
}
