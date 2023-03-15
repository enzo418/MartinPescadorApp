using FluentValidation;

namespace FisherTournament.Application.Tournaments.Commands.AddInscription;

public class AddInscriptionCommandValidator : AbstractValidator<AddInscriptionCommand>
{
    public AddInscriptionCommandValidator()
    {
        RuleFor(c => c.TournamentId.ToString()).NotEmpty();
        RuleFor(c => c.FisherId.ToString()).NotEmpty();
    }
}