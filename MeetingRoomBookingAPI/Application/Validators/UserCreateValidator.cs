using FluentValidation;
using MeetingRoomBookingAPI.Application.DTOs.User;

namespace MeetingRoomBookingAPI.Application.Validators
{
    public class UserCreateValidator : AbstractValidator<UserCreateDto>
    {
        private static readonly string[] AllowedRoles = { "User", "Admin" };

        public UserCreateValidator()
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

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role wajib diisi")
                .Must(role => AllowedRoles.Contains(role))
                .WithMessage("Role harus 'User' atau 'Admin'");
        }
    }
}
