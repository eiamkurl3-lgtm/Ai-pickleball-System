using AI_Based_Pickleball_Tournament_Management.DTOs;
using FluentValidation;

namespace AI_Based_Pickleball_Tournament_Management.Validations
{
    public class CreatePlayerDTOValidator : AbstractValidator<CreatePlayerDTO>
    {
        public static readonly string[] AllowedStatuses = { "Active", "Inactive", "Suspended" };

        public CreatePlayerDTOValidator()
        {
            RuleFor(p => p.UserName)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .MaximumLength(256).WithMessage(ValidationUtilities.MessageMaxLength)
                .Matches(@"^[a-zA-Z0-9._@-]+$").WithMessage(ValidationUtilities.InvalidUserNameMessage);

            RuleFor(p => p.DateOfBirth)
                .GreaterThanOrEqualTo(new DateTime(1900, 1, 1))
                .WithMessage(ValidationUtilities.GreaterThanDate(new DateTime(1900, 1, 1)))
                .LessThanOrEqualTo(DateTime.Today)
                .WithMessage(ValidationUtilities.LessThanDate(DateTime.Today))
                .When(p => p.DateOfBirth.HasValue);

            RuleFor(p => p.SkillLevel)
                .InclusiveBetween(1.0m, 7.0m)
                .WithMessage(ValidationUtilities.Between(1.0m, 7.0m, "SkillLevel"))
                .When(p => p.SkillLevel.HasValue);

            RuleFor(p => p.Status)
                .Must(p => p == null || AllowedStatuses.Contains(p))
                .WithMessage(ValidationUtilities.OneOf("Status", AllowedStatuses));
        }
    }
}
