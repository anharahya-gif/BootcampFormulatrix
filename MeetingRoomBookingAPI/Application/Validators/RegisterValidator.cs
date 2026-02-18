using FluentValidation;
using MeetingRoomBookingAPI.Application.DTOs.Auth;

namespace MeetingRoomBookingAPI.Application.Validators
{
    public class RegisterValidator : AbstractValidator<RegisterDto>
    {
        public RegisterValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email wajib diisi")
                .EmailAddress().WithMessage("Format email tidak valid");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password wajib diisi")
                .MinimumLength(6).WithMessage("Password minimal 6 karakter");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Nama lengkap wajib diisi")
                .MaximumLength(100);

            RuleFor(x => x.Department)
                .MaximumLength(100)
                .When(x => x.Department != null);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .When(x => x.PhoneNumber != null);
        }
    }
}
