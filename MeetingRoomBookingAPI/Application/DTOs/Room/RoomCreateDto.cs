using System;

namespace MeetingRoomBookingAPI.Application.DTOs.Room
{
    public class RoomCreateDto
    {
        public required string Name { get; set; }
        public int Capacity { get; set; }
        public required string Location { get; set; }
        public bool HasProjector { get; set; }
    }

}
