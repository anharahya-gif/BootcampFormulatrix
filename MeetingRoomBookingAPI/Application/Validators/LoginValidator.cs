using FluentValidation;
using MeetingRoomBookingAPI.Application.DTOs.Auth;

namespace MeetingRoomBookingAPI.Application.Validators
{
    public class LoginValidator : AbstractValidator<LoginDto>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email wajib diisi")
                .EmailAddress().WithMessage("Format email tidak valid");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password wajib diisi");
        }
    }
}
