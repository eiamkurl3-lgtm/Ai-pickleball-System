using AI_Based_Pickleball_Tournament_Management.DTOs;
using FluentValidation;

namespace AI_Based_Pickleball_Tournament_Management.Validations
{
    public class UpdateTournamentDTOValidator : AbstractValidator<UpdateTournamentDTO>
    {
        public static readonly string[] AllowedStatuses = { "Scheduled", "Active", "Completed", "Cancelled" };

        public UpdateTournamentDTOValidator()
        {
            RuleFor(p => p.Status)
                .NotEmpty().WithMessage(ValidationUtilities.NonEmptyMessage)
                .MaximumLength(50).WithMessage(ValidationUtilities.MessageMaxLength)
                .Must(status => AllowedStatuses.Contains(status))
                .WithMessage(ValidationUtilities.OneOf("Status", AllowedStatuses));
        }
    }
}
