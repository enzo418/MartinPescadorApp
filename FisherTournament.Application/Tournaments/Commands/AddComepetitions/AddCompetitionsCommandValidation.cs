using FisherTournament.Application.Common.Provider;
using FisherTournament.Application.Common.Validators;
using FluentValidation;

namespace FisherTournament.Application.Tournaments.Commands.AddCompetitions;

public class CompetitionCommandValidation : AbstractValidator<AddCompetitionCommand>
{
    public CompetitionCommandValidation(IDateTimeProvider dateTime)
    {
        RuleFor(c => c.StartDateTime)
            .IsUtcDateTime()
            .GreaterThan(dateTime.UtcNow);
        RuleFor(c => c.City).NotEmpty();
        RuleFor(c => c.State).NotEmpty();
        RuleFor(c => c.Country).NotEmpty();
        RuleFor(c => c.Place).NotEmpty();
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