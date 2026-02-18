using FluentValidation;
using MeetingRoomBookingAPI.Application.DTOs.User;

namespace MeetingRoomBookingAPI.Application.Validators
{
    public class UserUpdateValidator : AbstractValidator<UserUpdateDto>
    {
        public UserUpdateValidator()
        {
            RuleFor(x => x.FullName)
                .MaximumLength(100)
                .When(x => x.FullName != null);

            RuleFor(x => x.Department)
                .MaximumLength(100)
                .When(x => x.Department != null);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(20)
                .When(x => x.PhoneNumber != null);

            RuleFor(x => x.AvatarUrl)
                .MaximumLength(500)
                .When(x => x.AvatarUrl != null);
        }
    }
}
