using FluentValidation;

namespace FisherTournament.Application.Competitions.Commands.AddParticipation;

public class AddParticipationCommandValidator : AbstractValidator<AddParticipationCommand>
{
    public AddParticipationCommandValidator()
    {
        RuleFor(x => x.CompetitionId)
            .NotEmpty();

        RuleFor(x => x.FisherId)
            .NotEmpty();
    }
}