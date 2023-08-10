using FluentValidation;

namespace FisherTournament.Application.Tournaments.Commands.AddInscription;

public class AddSimpleInscriptionCommandValidator : AbstractValidator<AddSimpleInscriptionCommand>
{
    public AddSimpleInscriptionCommandValidator()
    {
        RuleFor(c => c.TournamentId).NotEmpty();
        RuleFor(c => c.FisherFirstName).NotEmpty();
        RuleFor(c => c.FisherLastName).NotEmpty();
        RuleFor(c => c.CategoryId).NotEmpty();
    }
}