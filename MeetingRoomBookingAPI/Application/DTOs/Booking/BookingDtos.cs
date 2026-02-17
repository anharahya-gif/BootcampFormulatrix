using System;
using System.Collections.Generic;
using MeetingRoomBookingAPI.Domain.Enums;

namespace MeetingRoomBookingAPI.Application.DTOs.Booking
{
    public class BookingReadDto
    {
        public Guid Id { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public Guid RoomId { get; set; }
        public string? RoomName { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string? CreatedByUserName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public BookingStatus Status { get; set; }
        public List<ParticipantDto> Participants { get; set; } = new();
    }

    public class BookingCreateDto
    {
        public required string Title { get; set; }
        public string? Description { get; set; }
        public Guid RoomId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<Guid> ParticipantUserIds { get; set; } = new();
    }

    public class ParticipantDto
    {
        public Guid UserId { get; set; }
        public required string FullName { get; set; }
        public bool IsOptional { get; set; }
    }
}
