using AI_Based_Pickleball_Tournament_Management.DTOs;
using FluentValidation;

namespace AI_Based_Pickleball_Tournament_Management.Validations
{
    public class UpdateMatchDTOValidator : AbstractValidator<UpdateMatchDTO>
    {
        public UpdateMatchDTOValidator()
        {
            RuleFor(p => p.Status)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .Must(status => CreateMatchDTOValidator.AllowedStatuses.Contains(status))
                .WithMessage(ValidationUtilities.OneOf("Status", CreateMatchDTOValidator.AllowedStatuses));

            RuleFor(p => p.SideAScore)
                .GreaterThanOrEqualTo(0).WithMessage(ValidationUtilities.GreaterThanOrEqualTo(0, "SideAScore"));

            RuleFor(p => p.SideBScore)
                .GreaterThanOrEqualTo(0).WithMessage(ValidationUtilities.GreaterThanOrEqualTo(0, "SideBScore"));

            ApplyCompletedResultRules();
        }

        // A completed match must carry a final, non-tied result
        private void ApplyCompletedResultRules()
        {
            RuleFor(p => p.SideBScore)
                .Must((match, sideBScore) => match.SideAScore != sideBScore)
                .WithMessage("A completed match cannot end in a tie, SideAScore and SideBScore must differ")
                .When(p => p.Status == "Completed");
        }
    }
}
