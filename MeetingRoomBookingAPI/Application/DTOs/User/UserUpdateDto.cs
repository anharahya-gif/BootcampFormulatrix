using System;

namespace MeetingRoomBookingAPI.Application.DTOs.User
{
    public class UserUpdateDto
    {
        public string? FullName { get; set; }
        public string? Department { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }
    }
}
