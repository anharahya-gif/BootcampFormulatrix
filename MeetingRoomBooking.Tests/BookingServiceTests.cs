using NUnit.Framework;
using Moq;
using AutoMapper;
using MeetingRoomBookingAPI.Application.Services;
using MeetingRoomBookingAPI.Application.Interfaces;
using MeetingRoomBookingAPI.Application.DTOs.Booking;
using MeetingRoomBookingAPI.Domain.Entities;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MeetingRoomBooking.Tests
{
    [TestFixture]
    public class BookingServiceTests
    {
        private Mock<IBookingRepository> _bookingRepoMock;
        private Mock<IGenericRepository<Room>> _roomRepoMock;
        private Mock<IMapper> _mapperMock;
        private BookingService _bookingService;

        [SetUp]
        public void Setup()
        {
            _bookingRepoMock = new Mock<IBookingRepository>();
            _roomRepoMock = new Mock<IGenericRepository<Room>>();
            _mapperMock = new Mock<IMapper>();

            _bookingService = new BookingService(
                _bookingRepoMock.Object,
                _roomRepoMock.Object,
                _mapperMock.Object
            );
        }

        [Test]
        public async Task CreateBookingAsync_ShouldReturnSuccess_WhenNoOverlap()
        {
            // Arrange
            var roomId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var createDto = new BookingCreateDto 
            { 
                RoomId = roomId, 
                StartTime = DateTime.UtcNow.AddHours(1), 
                EndTime = DateTime.UtcNow.AddHours(2),
                ParticipantUserIds = new List<Guid>()
            };

            var room = new Room { Id = roomId, Capacity = 10 };
            
            _roomRepoMock.Setup(r => r.GetByIdAsync(roomId))
                .ReturnsAsync(room);

            // a. Berhasil melakukan booking jika tidak ada jadwal yang bentrok.
            _bookingRepoMock.Setup(b => b.HasOverlapAsync(roomId, createDto.StartTime, createDto.EndTime))
                .ReturnsAsync(false); 

            var booking = new Booking { Id = Guid.NewGuid() };
            _mapperMock.Setup(m => m.Map<Booking>(createDto))
                .Returns(booking);

            var readDto = new BookingReadDto { Id = booking.Id };
            _mapperMock.Setup(m => m.Map<BookingReadDto>(It.IsAny<Booking>()))
                .Returns(readDto);

            // Act
            var result = await _bookingService.CreateBookingAsync(createDto, userId);

            // Assert
            Assert.IsTrue(result.Success, "Expected successful booking when there is no overlap.");
            Assert.AreEqual(201, result.StatusCode);
            _bookingRepoMock.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Once);
            _bookingRepoMock.Verify(b => b.SaveChangesAsync(), Times.Once);
        }

        [Test]
        public async Task CreateBookingAsync_ShouldReturnFailure_WhenTimeOverlaps()
        {
            // Arrange
            var roomId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var createDto = new BookingCreateDto 
            { 
                RoomId = roomId, 
                StartTime = DateTime.UtcNow.AddHours(1), 
                EndTime = DateTime.UtcNow.AddHours(2)
            };

            var room = new Room { Id = roomId, Capacity = 10 };
            
            _roomRepoMock.Setup(r => r.GetByIdAsync(roomId))
                .ReturnsAsync(room);

            // b. Gagal melakukan booking jika waktu mulai atau selesai tumpang tindih
            _bookingRepoMock.Setup(b => b.HasOverlapAsync(roomId, createDto.StartTime, createDto.EndTime))
                .ReturnsAsync(true); // Simulate overlapping schedule

            // Act
            var result = await _bookingService.CreateBookingAsync(createDto, userId);

            // Assert
            Assert.IsFalse(result.Success, "Expected booking to fail due to overlap.");
            Assert.AreEqual("Room is already booked for this time slot.", result.Message);
            _bookingRepoMock.Verify(b => b.AddAsync(It.IsAny<Booking>()), Times.Never);
            _bookingRepoMock.Verify(b => b.SaveChangesAsync(), Times.Never);
        }
    }
}
