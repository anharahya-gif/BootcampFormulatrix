using FluentValidation;
using MeetingRoomBookingAPI.Application.DTOs.Booking;

namespace MeetingRoomBookingAPI.Application.Validators
{
    public class BookingCreateValidator : AbstractValidator<BookingCreateDto>
    {
        public BookingCreateValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title wajib diisi")
                .MaximumLength(200);

            RuleFor(x => x.RoomId)
                .NotEmpty().WithMessage("Room wajib dipilih");

            RuleFor(x => x.StartTime)
                .GreaterThan(DateTime.Now)
                .WithMessage("Start time harus di masa depan");

            RuleFor(x => x.EndTime)
                .GreaterThan(x => x.StartTime)
                .WithMessage("End time harus setelah start time");
        }
    }
}
