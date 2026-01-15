using System.ComponentModel.DataAnnotations;

namespace CobaWebAPI.DTOs.Users
{
    public class UpdateUserDto
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; } = string.Empty;
    }
}
