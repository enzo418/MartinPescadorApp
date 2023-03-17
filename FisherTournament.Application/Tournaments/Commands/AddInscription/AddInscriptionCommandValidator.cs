using FluentValidation;

namespace FisherTournament.Application.Tournaments.Commands.AddInscription;

public class AddInscriptionCommandValidator : AbstractValidator<AddInscriptionCommand>
{
    public AddInscriptionCommandValidator()
    {
        RuleFor(c => c.TournamentId).NotEmpty();
        RuleFor(c => c.FisherId).NotEmpty();
    }
}