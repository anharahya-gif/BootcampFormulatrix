using System;
using System.Collections.Generic;
using MeetingRoomBookingAPI.Domain.Enums;

namespace MeetingRoomBookingAPI.Application.DTOs.Booking
{
    public class ParticipantDto
    {
        public Guid UserId { get; set; }
        public required string FullName { get; set; }
        public bool IsOptional { get; set; }
    }
}
