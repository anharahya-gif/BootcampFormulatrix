using Microsoft.AspNetCore.Identity;

namespace PokerAPIMPwDB.Infrastructure.Persistence.Entities;

public class User : IdentityUser<Guid>
{
    public int Balance { get; set; } = 0; // coin atau deposit
    public bool isDeleted { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
