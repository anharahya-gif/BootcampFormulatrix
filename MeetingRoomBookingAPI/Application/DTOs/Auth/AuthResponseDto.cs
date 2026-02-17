using System.ComponentModel.DataAnnotations;

namespace MeetingRoomBookingAPI.Application.DTOs.Auth
{
    public class AuthResponseDto
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? ErrorMessage { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public Guid UserId { get; set; }
    }
}
