using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeetingRoomBookingAPI.Application.Common;
using MeetingRoomBookingAPI.Application.DTOs;

namespace MeetingRoomBookingAPI.Application.Interfaces
{
    public interface IRecycleBinService
    {
        Task<ServiceResult<IEnumerable<DeletedItemDto>>> GetDeletedItemsAsync();
        Task<ServiceResult<bool>> RestoreItemAsync(Guid id, string type);
        Task<ServiceResult<bool>> HardDeleteItemAsync(Guid id, string type);
    }
}
