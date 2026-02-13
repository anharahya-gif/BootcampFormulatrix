namespace PokerAPIMPwDB.DTO.User
{public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = null!;
    public int Balance { get; set; }
    public DateTime CreatedAt { get; set; }
}
}