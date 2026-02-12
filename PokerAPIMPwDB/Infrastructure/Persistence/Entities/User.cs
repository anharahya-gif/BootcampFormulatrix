namespace PokerAPIMPwDB.Infrastructure.Persistence.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public int Balance { get; set; } = 0; // coin atau deposit
    public bool isDeleted {get;set;}=false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
