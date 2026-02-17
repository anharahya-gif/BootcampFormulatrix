using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MeetingRoomBookingAPI.Application.Common;
using MeetingRoomBookingAPI.Application.DTOs.Room;
using MeetingRoomBookingAPI.Application.Interfaces;
using MeetingRoomBookingAPI.Domain.Entities;

namespace MeetingRoomBookingAPI.Application.Services
{
    public class RoomService : IRoomService
    {
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IMapper _mapper;

        public RoomService(IGenericRepository<Room> roomRepository, IMapper mapper)
        {
            _roomRepository = roomRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<IEnumerable<RoomReadDto>>> GetAllRoomsAsync()
        {
            var rooms = await _roomRepository.GetAllAsync();
            return ServiceResult<IEnumerable<RoomReadDto>>.SuccessResult(_mapper.Map<IEnumerable<RoomReadDto>>(rooms));
        }

        public async Task<ServiceResult<RoomReadDto>> GetRoomByIdAsync(Guid id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null) return ServiceResult<RoomReadDto>.FailureResult("Room not found", 404);
            return ServiceResult<RoomReadDto>.SuccessResult(_mapper.Map<RoomReadDto>(room));
        }

        public async Task<ServiceResult<RoomReadDto>> CreateRoomAsync(RoomCreateDto roomCreateDto)
        {
            var room = _mapper.Map<Room>(roomCreateDto);
            await _roomRepository.AddAsync(room);
            await _roomRepository.SaveChangesAsync();
            return ServiceResult<RoomReadDto>.SuccessResult(_mapper.Map<RoomReadDto>(room), 201);
        }

        public async Task<ServiceResult<RoomReadDto>> UpdateRoomAsync(Guid id, RoomUpdateDto roomUpdateDto)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null) return ServiceResult<RoomReadDto>.FailureResult("Room not found", 404);

            _mapper.Map(roomUpdateDto, room);
            _roomRepository.Update(room);
            await _roomRepository.SaveChangesAsync();

            return ServiceResult<RoomReadDto>.SuccessResult(_mapper.Map<RoomReadDto>(room));
        }

        public async Task<ServiceResult<bool>> DeleteRoomAsync(Guid id)
        {
            var room = await _roomRepository.GetByIdAsync(id);
            if (room == null) return ServiceResult<bool>.FailureResult("Room not found", 404);

            _roomRepository.Remove(room);
            await _roomRepository.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }
    }
}
