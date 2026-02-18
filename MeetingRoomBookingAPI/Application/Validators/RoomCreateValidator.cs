using FluentValidation;
using MeetingRoomBookingAPI.Application.DTOs.Room;

namespace MeetingRoomBookingAPI.Application.Validators
{
    public class RoomCreateValidator : AbstractValidator<RoomCreateDto>
    {
        public RoomCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nama ruangan wajib diisi")
                .MaximumLength(100);

            RuleFor(x => x.Capacity)
                .GreaterThan(0).WithMessage("Capacity harus lebih dari 0");
            
            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Lokasi ruangan wajib diisi")
                .MaximumLength(100);
        }
    }
}
