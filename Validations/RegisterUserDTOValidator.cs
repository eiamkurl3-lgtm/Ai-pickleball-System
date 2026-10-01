using AI_Based_Pickleball_Tournament_Management.DTOs;
using FluentValidation;

namespace AI_Based_Pickleball_Tournament_Management.Validations
{
    public class RegisterUserDTOValidator : AbstractValidator<RegisterUserDTO>
    {
        public RegisterUserDTOValidator()
        {
            RuleFor(p => p.UserName)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .MaximumLength(256).WithMessage(ValidationUtilities.MessageMaxLength)
                .Matches(@"^[a-zA-Z0-9._@-]+$").WithMessage(ValidationUtilities.InvalidUserNameMessage);

            RuleFor(p => p.Password)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .MinimumLength(6).WithMessage(ValidationUtilities.MinLengthMessage);
        }
    }
}