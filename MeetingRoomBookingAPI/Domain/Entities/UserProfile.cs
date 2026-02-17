using System;
using MeetingRoomBookingAPI.Domain.Common;

namespace MeetingRoomBookingAPI.Domain.Entities
{
    public class UserProfile : BaseEntity
    {
        public Guid UserId { get; set; }
        public required string FullName { get; set; }
        public string? Department { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }

        // Navigation property
        public virtual ApplicationUser? User { get; set; }
    }
}
