using AI_Based_Pickleball_Tournament_Management.DTOs;
using FluentValidation;

namespace AI_Based_Pickleball_Tournament_Management.Validations
{
    public class CreateMatchDTOValidator : AbstractValidator<CreateMatchDTO>
    {
        public static readonly string[] AllowedStatuses = { "Scheduled", "InProgress", "Completed", "Cancelled" };

        public CreateMatchDTOValidator()
        {
            RuleFor(p => p.TournamentId)
                .GreaterThan(0).WithMessage(ValidationUtilities.GreaterThan(0, "TournamentId"));

            RuleFor(p => p.UserName)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .MaximumLength(256).WithMessage(ValidationUtilities.MessageMaxLength)
                .Matches(@"^[a-zA-Z0-9._@-]+$").WithMessage(ValidationUtilities.InvalidUserNameMessage);

            RuleFor(p => p.UserName2)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .MaximumLength(256).WithMessage(ValidationUtilities.MessageMaxLength)
                .Matches(@"^[a-zA-Z0-9._@-]+$").WithMessage(ValidationUtilities.InvalidUserNameMessage)
                .Must((match, userName2) => userName2 != match.UserName)
                .WithMessage("Field UserName2 must be a different player than UserName");

            RuleFor(p => p.ScheduledStart)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage);

            RuleFor(p => p.Status)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .Must(status => AllowedStatuses.Contains(status))
                .WithMessage(ValidationUtilities.OneOf("Status", AllowedStatuses));

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
