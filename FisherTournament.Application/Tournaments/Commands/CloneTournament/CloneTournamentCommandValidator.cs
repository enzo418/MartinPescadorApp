using FisherTournament.Application.Common.Validators;
using FisherTournament.Domain.Common.Provider;
using FluentValidation;

namespace FisherTournament.Application.Tournaments.Commands.CloneTournament;

public class CloneTournamentCommandValidator : AbstractValidator<CloneTournamentCommand>
{
	public CloneTournamentCommandValidator(IDateTimeProvider dateTime)
	{
		RuleFor(c => c.SourceTournamentId)
			.NotEmpty();

		RuleFor(c => c.NewTournamentStartDate)
			.IsUtcDateTime();

		RuleFor(c => c.NewTournamentName)
			.NotEmpty();
	}
}