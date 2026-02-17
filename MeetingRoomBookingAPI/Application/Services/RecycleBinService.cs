using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MeetingRoomBookingAPI.Application.Common;
using MeetingRoomBookingAPI.Application.DTOs;
using MeetingRoomBookingAPI.Application.Interfaces;
using MeetingRoomBookingAPI.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MeetingRoomBookingAPI.Application.Services
{
    public class RecycleBinService : IRecycleBinService
    {
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IGenericRepository<Booking> _bookingRepository;
        private readonly IGenericRepository<UserProfile> _userProfileRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public RecycleBinService(
            IGenericRepository<Room> roomRepository,
            IGenericRepository<Booking> bookingRepository,
            IGenericRepository<UserProfile> userProfileRepository,
            UserManager<ApplicationUser> userManager)
        {
            _roomRepository = roomRepository;
            _bookingRepository = bookingRepository;
            _userProfileRepository = userProfileRepository;
            _userManager = userManager;
        }

        public async Task<ServiceResult<IEnumerable<DeletedItemDto>>> GetDeletedItemsAsync()
        {
            var deletedItems = new List<DeletedItemDto>();

            // Fetch Deleted Rooms
            var rooms = await _roomRepository.GetDeletedAsync();
            deletedItems.AddRange(rooms.Select(r => new DeletedItemDto
            {
                Id = r.Id,
                Name = r.Name,
                Type = "Room",
                DeletedAt = r.DeletedAt ?? DateTime.UtcNow
            }));

            // Fetch Deleted Bookings
            var bookings = await _bookingRepository.GetDeletedAsync();
            deletedItems.AddRange(bookings.Select(b => new DeletedItemDto
            {
                Id = b.Id,
                Name = b.Title,
                Type = "Booking",
                DeletedAt = b.DeletedAt ?? DateTime.UtcNow
            }));

            // Fetch Deleted UserProfiles
            // Link directly to UserProfile to get the name
            var profiles = await _userProfileRepository.GetDeletedAsync();
            deletedItems.AddRange(profiles.Select(p => new DeletedItemDto
            {
                Id = p.Id,
                Name = p.FullName,
                Type = "User",
                DeletedAt = p.DeletedAt ?? DateTime.UtcNow
            }));

            return ServiceResult<IEnumerable<DeletedItemDto>>.SuccessResult(deletedItems.OrderByDescending(x => x.DeletedAt));
        }

        public async Task<ServiceResult<bool>> RestoreItemAsync(Guid id, string type)
        {
            switch (type?.ToLower())
            {
                case "room":
                    await _roomRepository.RestoreAsync(id);
                    break;
                case "booking":
                    await _bookingRepository.RestoreAsync(id);
                    break;
                case "user":
                    var profile = (await _userProfileRepository.GetDeletedAsync()).FirstOrDefault(p => p.Id == id);
                    if (profile != null)
                    {
                        var user = await _userManager.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == profile.UserId);
                        if (user != null)
                        {
                            user.IsDeleted = false;
                            user.DeletedAt = null;
                            await _userManager.UpdateAsync(user);
                        }
                        await _userProfileRepository.RestoreAsync(id);
                    }
                    break;
                default:
                    return ServiceResult<bool>.FailureResult("Invalid item type", 400);
            }

            return ServiceResult<bool>.SuccessResult(true);
        }

        public async Task<ServiceResult<bool>> HardDeleteItemAsync(Guid id, string type)
        {
            switch (type?.ToLower())
            {
                case "room":
                    await _roomRepository.HardDeleteAsync(id);
                    break;
                case "booking":
                    await _bookingRepository.HardDeleteAsync(id);
                    break;
                case "user":
                    var profile = (await _userProfileRepository.GetDeletedAsync()).FirstOrDefault(p => p.Id == id);
                    if (profile != null)
                    {
                        var user = await _userManager.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == profile.UserId);
                        await _userProfileRepository.HardDeleteAsync(id);
                        if (user != null)
                        {
                            await _userManager.DeleteAsync(user);
                        }
                    }
                    break;
                default:
                    return ServiceResult<bool>.FailureResult("Invalid item type", 400);
            }

            return ServiceResult<bool>.SuccessResult(true);
        }
    }
}
