using FluentValidation;

namespace FisherTournament.Application.Fishers.Commands.EditFisher;

public class EditFisherCommandValidator : AbstractValidator<EditFisherCommand>
{
    public EditFisherCommandValidator()
    {
        RuleFor(c => c.FisherId).NotEmpty();
        RuleFor(c => c.NewFirstName).NotEmpty();
        RuleFor(c => c.NewLastName).NotEmpty();
        RuleFor(c => c.NewDNI).NotEmpty();
    }
}