using System;

namespace MeetingRoomBookingAPI.Application.DTOs.User
{
    public class UserReadDto
    {
        public Guid Id { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public string? FullName { get; set; }
        public string? Department { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
