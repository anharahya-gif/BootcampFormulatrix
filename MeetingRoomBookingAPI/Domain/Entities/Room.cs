using System.Collections.Generic;
using MeetingRoomBookingAPI.Domain.Common;

namespace MeetingRoomBookingAPI.Domain.Entities
{
    public class Room : BaseEntity
    {
        public required string Name { get; set; }
        public int Capacity { get; set; }
        public required string Location { get; set; }
        public bool HasProjector { get; set; }

        // Navigation property
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
