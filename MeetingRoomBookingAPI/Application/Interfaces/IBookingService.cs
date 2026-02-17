using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeetingRoomBookingAPI.Application.Common;
using MeetingRoomBookingAPI.Application.DTOs.Booking;
using MeetingRoomBookingAPI.Domain.Enums;

namespace MeetingRoomBookingAPI.Application.Interfaces
{
    public interface IBookingService
    {
        Task<ServiceResult<BookingReadDto>> CreateBookingAsync(BookingCreateDto bookingCreateDto, Guid userId);
        Task<ServiceResult<IEnumerable<BookingReadDto>>> GetUserBookingsAsync(Guid userId);
        Task<ServiceResult<BookingReadDto>> GetBookingByIdAsync(Guid id);
        Task<ServiceResult<bool>> CancelBookingAsync(Guid id, Guid userId);
        Task<ServiceResult<bool>> UpdateBookingStatusAsync(Guid id, BookingStatus status);
        Task<ServiceResult<IEnumerable<BookingReadDto>>> GetAllBookingsAsync();
    }
}
