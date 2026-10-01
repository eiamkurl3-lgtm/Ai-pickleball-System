using AI_Based_Pickleball_Tournament_Management.DTOs;
using FluentValidation;

namespace AI_Based_Pickleball_Tournament_Management.Validations
{
    public class EditClaimDTOValidator : AbstractValidator<EditClaimDTO>
    {
        public EditClaimDTOValidator()
        {
            RuleFor(p => p.UserName)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .MaximumLength(256).WithMessage(ValidationUtilities.MessageMaxLength)
                .Matches(@"^[a-zA-Z0-9._@-]+$").WithMessage(ValidationUtilities.InvalidUserNameMessage);
        }
    }
}