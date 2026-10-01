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

            RuleFor(p => p.Player1Id)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .MaximumLength(450).WithMessage(ValidationUtilities.MessageMaxLength);

            RuleFor(p => p.Player2Id)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .MaximumLength(450).WithMessage(ValidationUtilities.MessageMaxLength)
                .Must((match, player2Id) => player2Id == null || player2Id != match.Player1Id)
                .WithMessage("Field Player2Id must be a different player than Player1Id");

            RuleFor(p => p.Player3Id)
                .MaximumLength(450).WithMessage(ValidationUtilities.MessageMaxLength)
                .Must((match, player3Id) => player3Id == null || player3Id != match.Player1Id)
                .WithMessage("Field Player3Id must be a different player than Player1Id")
                .Must((match, player3Id) => player3Id == null || player3Id != match.Player2Id)
                .WithMessage("Field Player3Id must be a different player than Player2Id");

            RuleFor(p => p.Player4Id)
                .MaximumLength(450).WithMessage(ValidationUtilities.MessageMaxLength)
                .Must((match, player4Id) => player4Id == null || player4Id != match.Player1Id)
                .WithMessage("Field Player4Id must be a different player than Player1Id")
                .Must((match, player4Id) => player4Id == null || player4Id != match.Player2Id)
                .WithMessage("Field Player4Id must be a different player than Player2Id")
                .Must((match, player4Id) => player4Id == null || player4Id != match.Player3Id)
                .WithMessage("Field Player4Id must be a different player than Player3Id");

            RuleFor(p => p.ScheduledStart)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage);

            RuleFor(p => p.Court)
                .MaximumLength(100).WithMessage(ValidationUtilities.MessageMaxLength);

            RuleFor(p => p.Status)
                .Must(p => p == null || AllowedStatuses.Contains(p))
                .WithMessage($"Field Status must be one of: {string.Join(", ", AllowedStatuses)}");

            RuleFor(p => p.SideAScore)
                .GreaterThanOrEqualTo(0).WithMessage(ValidationUtilities.GreaterThanOrEqualTo(0, "SideAScore"))
                .When(p => p.SideAScore.HasValue);

            RuleFor(p => p.SideBScore)
                .GreaterThanOrEqualTo(0).WithMessage(ValidationUtilities.GreaterThanOrEqualTo(0, "SideBScore"))
                .When(p => p.SideBScore.HasValue);

            RuleFor(p => p.WinnerSide)
                .Must(p => p == null || p == "A" || p == "B")
                .WithMessage("Field WinnerSide must be either A or B");

            ApplyCompletedResultRules();
        }

        // A completed match must carry a final, non-tied, self-consistent result
        private void ApplyCompletedResultRules()
        {
            RuleFor(p => p.SideAScore)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .When(p => p.Status == "Completed");

            RuleFor(p => p.SideBScore)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .When(p => p.Status == "Completed");

            RuleFor(p => p.SideBScore)
                .Must((match, sideBScore) => !match.SideAScore.HasValue || !sideBScore.HasValue
                    || match.SideAScore != sideBScore)
                .WithMessage("A completed match cannot end in a tie, SideAScore and SideBScore must differ")
                .When(p => p.Status == "Completed");

            RuleFor(p => p.WinnerSide)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .Must((match, winnerSide) => !match.SideAScore.HasValue || !match.SideBScore.HasValue
                    || match.SideAScore == match.SideBScore
                    || winnerSide == (match.SideAScore > match.SideBScore ? "A" : "B"))
                .WithMessage("Field WinnerSide must be the side that has the higher score")
                .When(p => p.Status == "Completed");
        }
    }
}
