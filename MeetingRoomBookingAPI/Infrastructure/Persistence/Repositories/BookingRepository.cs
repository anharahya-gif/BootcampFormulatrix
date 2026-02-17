using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeetingRoomBookingAPI.Application.Interfaces;
using MeetingRoomBookingAPI.Domain.Entities;
using MeetingRoomBookingAPI.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MeetingRoomBookingAPI.Infrastructure.Persistence.Repositories
{
    public class BookingRepository : GenericRepository<Booking>, IBookingRepository
    {
        public BookingRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<bool> HasOverlapAsync(Guid roomId, DateTime startTime, DateTime endTime, Guid? excludeBookingId = null)
        {
            var query = _dbSet.Where(b => b.RoomId == roomId && b.Status != BookingStatus.Cancelled);

            if (excludeBookingId.HasValue)
            {
                query = query.Where(b => b.Id != excludeBookingId.Value);
            }

            return await query.AnyAsync(b => 
                (startTime >= b.StartTime && startTime < b.EndTime) || 
                (endTime > b.StartTime && endTime <= b.EndTime) ||
                (startTime <= b.StartTime && endTime >= b.EndTime)
            );
        }

        public async Task<Booking?> GetBookingWithDetailsAsync(Guid id)
        {
            return await _dbSet
                .Include(b => b.Room)
                .Include(b => b.CreatedByUser)
                .Include(b => b.Participants)
                    .ThenInclude(p => p.User)
                        .ThenInclude(u => u!.Profile)
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Where(b => b.CreatedByUserId == userId)
                .Include(b => b.Room)
                .ToListAsync();
        }

        public async Task<IEnumerable<Booking>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(b => b.Room)
                .Include(b => b.CreatedByUser)
                .ToListAsync();
        }
    }
}
