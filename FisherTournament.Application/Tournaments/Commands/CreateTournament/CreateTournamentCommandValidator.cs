using FisherTournament.Application.Common.Provider;
using FisherTournament.Application.Common.Validators;
using FisherTournament.Application.Tournaments.Commands.CreateTournament;
using FluentValidation;

namespace FisherTournament.Application.Tournaments.Commands.AddInscription;

public class CreateTournamentCommandValidator : AbstractValidator<CreateTournamentCommand>
{
    public CreateTournamentCommandValidator(IDateTimeProvider dateTime)
    {
        RuleFor(c => c.Name).NotEmpty();

        RuleFor(c => c.StartDate)
            .NotEmpty()
            .IsUtcDateTime()
            .LessThan(c => c.EndDate)
            .GreaterThanOrEqualTo(dateTime.UtcNow);

        RuleFor(c => c.EndDate)
            .NotEmpty()
            .IsUtcDateTime();
    }
}