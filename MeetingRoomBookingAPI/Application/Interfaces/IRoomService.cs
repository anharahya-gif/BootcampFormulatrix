using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MeetingRoomBookingAPI.Application.Common;
using MeetingRoomBookingAPI.Application.DTOs.Room;

namespace MeetingRoomBookingAPI.Application.Interfaces
{
    public interface IRoomService
    {
        Task<ServiceResult<IEnumerable<RoomReadDto>>> GetAllRoomsAsync();
        Task<ServiceResult<RoomReadDto>> GetRoomByIdAsync(Guid id);
        Task<ServiceResult<RoomReadDto>> CreateRoomAsync(RoomCreateDto roomCreateDto);
        Task<ServiceResult<RoomReadDto>> UpdateRoomAsync(Guid id, RoomUpdateDto roomUpdateDto);
        Task<ServiceResult<bool>> DeleteRoomAsync(Guid id);
    }
}
