

namespace PokerAPIMPwDB.DTO.User
{public class CreateUserDto
{
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public int Balance { get; set; }
}
}