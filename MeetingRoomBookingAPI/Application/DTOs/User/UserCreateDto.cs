using System;

namespace MeetingRoomBookingAPI.Application.DTOs.User
{
    public class UserCreateDto
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
        public required string FullName { get; set; }
        public string? Department { get; set; }
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "User";
    }

}
