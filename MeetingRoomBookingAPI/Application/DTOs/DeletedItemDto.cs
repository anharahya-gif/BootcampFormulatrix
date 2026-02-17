using System;

namespace MeetingRoomBookingAPI.Application.DTOs
{
    public class DeletedItemDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; } // "Room", "Booking", "User"
        public DateTime DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
