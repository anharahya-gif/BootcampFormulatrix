using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeetingRoomBookingAPI.Domain.Entities;

namespace MeetingRoomBookingAPI.Application.Interfaces
{
    public interface IBookingRepository : IGenericRepository<Booking>
    {
        Task<bool> HasOverlapAsync(Guid roomId, DateTime startTime, DateTime endTime, Guid? excludeBookingId = null);
        Task<Booking?> GetBookingWithDetailsAsync(Guid id);
        Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(Guid userId);
        Task<IEnumerable<Booking>> GetAllWithDetailsAsync();
    }
}
