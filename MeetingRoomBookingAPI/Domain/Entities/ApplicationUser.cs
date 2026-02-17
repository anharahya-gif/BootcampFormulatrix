using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using MeetingRoomBookingAPI.Domain.Common;

namespace MeetingRoomBookingAPI.Domain.Entities
{
    public class ApplicationUser : IdentityUser<Guid>, ISoftDelete
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual UserProfile? Profile { get; set; }
        public virtual ICollection<Booking> BookingsCreated { get; set; } = new List<Booking>();
        public virtual ICollection<BookingParticipant> BookingParticipants { get; set; } = new List<BookingParticipant>();
    }
}
