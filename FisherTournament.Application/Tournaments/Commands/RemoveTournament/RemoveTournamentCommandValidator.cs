using FluentValidation;

namespace FisherTournament.Application.Tournaments.Commands.RemoveTournament
{
    public class RemoveTournamentCommandValidator : AbstractValidator<RemoveTournamentCommand>
    {
        public RemoveTournamentCommandValidator()
        {
            RuleFor(c => c.TournamentId).NotEmpty();
        }
    }
}