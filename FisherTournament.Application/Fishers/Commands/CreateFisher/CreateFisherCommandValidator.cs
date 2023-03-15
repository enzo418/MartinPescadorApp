using FluentValidation;

namespace FisherTournament.Application.Fishers.Commands.CreateFisher;

public class CreateFisherCommandValidator : AbstractValidator<CreateFisherCommand>
{
    public CreateFisherCommandValidator()
    {
        RuleFor(c => c.FirstName).NotEmpty();
        RuleFor(c => c.LastName).NotEmpty();
    }
}