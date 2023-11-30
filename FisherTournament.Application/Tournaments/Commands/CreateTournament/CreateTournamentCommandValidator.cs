using FisherTournament.Application.Common.Validators;
using FisherTournament.Application.Tournaments.Commands.CreateTournament;
using FisherTournament.Domain.Common.Provider;
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
            .LessThan(c => c.EndDate ?? DateTime.MaxValue)
            .GreaterThanOrEqualTo(dateTime.Now);

        /*RuleFor(c => c.EndDate)
            .NotEmpty()
            .IsUtcDateTime();*/
    }
}