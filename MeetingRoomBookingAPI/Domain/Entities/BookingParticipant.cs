using System;
using MeetingRoomBookingAPI.Domain.Common;

namespace MeetingRoomBookingAPI.Domain.Entities
{
    public class BookingParticipant : BaseEntity
    {
        public Guid BookingId { get; set; }
        public Guid UserId { get; set; }
        public bool IsOptional { get; set; }

        // Navigation properties
        public virtual Booking? Booking { get; set; }
        public virtual ApplicationUser? User { get; set; }
    }
}
