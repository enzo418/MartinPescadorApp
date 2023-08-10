using FisherTournament.Application.Common.Validators;
using FisherTournament.Domain.Common.Provider;
using FluentValidation;

namespace FisherTournament.Application.Tournaments.Commands.AddCompetitions;

public class CompetitionCommandValidation : AbstractValidator<AddCompetitionCommand>
{
    public CompetitionCommandValidation(IDateTimeProvider dateTime)
    {
        RuleFor(c => c.StartDateTime)
            .IsUtcDateTime()
            .GreaterThan(dateTime.Now);
    }
}

public class AddCompetitionsCommandValidation : AbstractValidator<AddCompetitionsCommand>
{
    public AddCompetitionsCommandValidation(CompetitionCommandValidation competitionCommandValidation, IDateTimeProvider inj)
    {
        RuleFor(c => c.TournamentId).NotEmpty();
        RuleFor(c => c.Competitions).NotEmpty();
        RuleForEach(c => c.Competitions)
            .NotEmpty()
            .SetValidator(competitionCommandValidation);
    }
}