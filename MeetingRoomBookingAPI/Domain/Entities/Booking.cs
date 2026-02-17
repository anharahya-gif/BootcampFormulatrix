using System;
using System.Collections.Generic;
using MeetingRoomBookingAPI.Domain.Common;
using MeetingRoomBookingAPI.Domain.Enums;

namespace MeetingRoomBookingAPI.Domain.Entities
{
    public class Booking : BaseEntity
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public Guid RoomId { get; set; }
        public Guid CreatedByUserId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public BookingStatus Status { get; set; }

        // Navigation properties
        public virtual Room? Room { get; set; }
        public virtual ApplicationUser? CreatedByUser { get; set; }
        public virtual ICollection<BookingParticipant> Participants { get; set; } = new List<BookingParticipant>();
    }
}
