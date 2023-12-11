using FluentValidation;

namespace FisherTournament.Application.Tournaments.Commands.EditInscription
{
    public class EditInscriptionCommandValidator : AbstractValidator<EditInscriptionCommand>
    {
        public EditInscriptionCommandValidator()
        {
            RuleFor(c => c.TournamentId).NotEmpty();
            RuleFor(c => c.FisherId).NotEmpty();
        }
    }
}