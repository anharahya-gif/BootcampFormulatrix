using System;
using System.Collections.Generic;
using MeetingRoomBookingAPI.Domain.Enums;

namespace MeetingRoomBookingAPI.Application.DTOs.Booking
{
    public class BookingCreateDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public Guid RoomId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<Guid> ParticipantUserIds { get; set; } = new();
    }

}
