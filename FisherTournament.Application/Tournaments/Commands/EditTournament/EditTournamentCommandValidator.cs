using FisherTournament.Application.Common.Validators;
using FisherTournament.Domain.Common.Provider;
using FluentValidation;

namespace FisherTournament.Application.Tournaments.Commands.EditTournament;

public class EditTournamentCommandValidator : AbstractValidator<EditTournamentCommand>
{
    public EditTournamentCommandValidator(IDateTimeProvider dateTime)
    {
        RuleFor(c => c.TournamentId)
            .NotEmpty();

        RuleFor(c => c.Name)
            .NotEmpty()
            .When(c => c.Name != null);

        RuleFor(c => c.StartDate)
            .IsUtcDateTime()
            /*.GreaterThanOrEqualTo(dateTime.Now)*/
            .When(c => c.StartDate is not null);
    }
}