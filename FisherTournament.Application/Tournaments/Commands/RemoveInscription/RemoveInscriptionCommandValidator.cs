using FluentValidation;

namespace FisherTournament.Application.Tournaments.Commands.RemoveInscription
{
    public class RemoveInscriptionCommandValidator : AbstractValidator<RemoveInscriptionCommand>
    {
        public RemoveInscriptionCommandValidator()
        {
            RuleFor(c => c.TournamentId).NotEmpty();
            RuleFor(c => c.FisherId).NotEmpty();
        }
    }
}