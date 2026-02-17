using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MeetingRoomBookingAPI.Application.Common;
using MeetingRoomBookingAPI.Application.DTOs.Booking;
using MeetingRoomBookingAPI.Application.Interfaces;
using MeetingRoomBookingAPI.Domain.Entities;
using MeetingRoomBookingAPI.Domain.Enums;

namespace MeetingRoomBookingAPI.Application.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IGenericRepository<Room> _roomRepository;
        private readonly IMapper _mapper;

        public BookingService(
            IBookingRepository bookingRepository, 
            IGenericRepository<Room> roomRepository,
            IMapper mapper)
        {
            _bookingRepository = bookingRepository;
            _roomRepository = roomRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult<BookingReadDto>> CreateBookingAsync(BookingCreateDto bookingCreateDto, Guid userId)
        {
            var room = await _roomRepository.GetByIdAsync(bookingCreateDto.RoomId);
            if (room == null) return ServiceResult<BookingReadDto>.FailureResult("Room not found", 404);

            int totalParticipants = (bookingCreateDto.ParticipantUserIds?.Count ?? 0) + 1;
            if (totalParticipants > room.Capacity)
            {
                return ServiceResult<BookingReadDto>.FailureResult($"Room capacity ({room.Capacity}) exceeded. Total participants: {totalParticipants}");
            }

            bool overlap = await _bookingRepository.HasOverlapAsync(bookingCreateDto.RoomId, bookingCreateDto.StartTime, bookingCreateDto.EndTime);
            if (overlap)
            {
                return ServiceResult<BookingReadDto>.FailureResult("Room is already booked for this time slot.");
            }

            var booking = _mapper.Map<Booking>(bookingCreateDto);
            booking.CreatedByUserId = userId;
            booking.Status = BookingStatus.Pending;

            if (bookingCreateDto.ParticipantUserIds != null)
            {
                foreach (var participantId in bookingCreateDto.ParticipantUserIds)
                {
                    booking.Participants.Add(new BookingParticipant
                    {
                        UserId = participantId,
                        IsOptional = false
                    });
                }
            }

            await _bookingRepository.AddAsync(booking);
            await _bookingRepository.SaveChangesAsync();

            var result = await _bookingRepository.GetBookingWithDetailsAsync(booking.Id);
            return ServiceResult<BookingReadDto>.SuccessResult(_mapper.Map<BookingReadDto>(result), 201);
        }

        public async Task<ServiceResult<IEnumerable<BookingReadDto>>> GetUserBookingsAsync(Guid userId)
        {
            var bookings = await _bookingRepository.GetBookingsByUserIdAsync(userId);
            return ServiceResult<IEnumerable<BookingReadDto>>.SuccessResult(_mapper.Map<IEnumerable<BookingReadDto>>(bookings));
        }

        public async Task<ServiceResult<BookingReadDto>> GetBookingByIdAsync(Guid id)
        {
            var booking = await _bookingRepository.GetBookingWithDetailsAsync(id);
            if (booking == null) return ServiceResult<BookingReadDto>.FailureResult("Booking not found", 404);

            return ServiceResult<BookingReadDto>.SuccessResult(_mapper.Map<BookingReadDto>(booking));
        }

        public async Task<ServiceResult<bool>> CancelBookingAsync(Guid id, Guid userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return ServiceResult<bool>.FailureResult("Booking not found", 404);

            if (booking.CreatedByUserId != userId)
            {
                return ServiceResult<bool>.FailureResult("You are not authorized to cancel this booking", 403);
            }

            booking.Status = BookingStatus.Cancelled;
            _bookingRepository.Update(booking);
            await _bookingRepository.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }

        public async Task<ServiceResult<bool>> UpdateBookingStatusAsync(Guid id, BookingStatus status)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking == null) return ServiceResult<bool>.FailureResult("Booking not found", 404);

            booking.Status = status;
            _bookingRepository.Update(booking);
            await _bookingRepository.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true);
        }

        public async Task<ServiceResult<IEnumerable<BookingReadDto>>> GetAllBookingsAsync()
        {
            var bookings = await _bookingRepository.GetAllWithDetailsAsync();
            return ServiceResult<IEnumerable<BookingReadDto>>.SuccessResult(_mapper.Map<IEnumerable<BookingReadDto>>(bookings));
        }
    }
}
