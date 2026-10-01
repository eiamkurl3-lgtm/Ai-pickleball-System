using AI_Based_Pickleball_Tournament_Management.DTOs;
using FluentValidation;

namespace AI_Based_Pickleball_Tournament_Management.Validations
{
    public class UpdatePlayerDTOValidator : AbstractValidator<UpdatePlayerDTO>
    {
        public UpdatePlayerDTOValidator()
        {
            RuleFor(p => p.SkillLevel)
                .InclusiveBetween(1.0m, 7.0m)
                .WithMessage(ValidationUtilities.Between(1.0m, 7.0m, "SkillLevel"))
                .When(p => p.SkillLevel.HasValue);

            RuleFor(p => p.Status)
                .Must(p => p == null || CreatePlayerDTOValidator.AllowedStatuses.Contains(p))
                .WithMessage(ValidationUtilities.OneOf("Status", CreatePlayerDTOValidator.AllowedStatuses));
        }
    }
}
