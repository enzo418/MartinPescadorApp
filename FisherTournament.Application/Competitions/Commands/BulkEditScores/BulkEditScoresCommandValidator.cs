using FluentValidation;

namespace FisherTournament.Application.Competitions.Commands.BulkEditScores
{
    public class BulkEditScoresItemValidator : AbstractValidator<BulkEditScoresItem>
    {
        public BulkEditScoresItemValidator()
        {
            RuleFor(c => c.FisherId).NotEmpty();
            RuleForEach(c => c.Scores).GreaterThan(0);
        }
    }

    public class BulkEditScoresCommandValidator : AbstractValidator<BulkEditScoresCommand>
    {
        private readonly BulkEditScoresItemValidator _itemValidator = new();

        public BulkEditScoresCommandValidator()
        {
            RuleFor(c => c.CompetitionId).NotEmpty();
            RuleFor(c => c.Items).NotEmpty();
            RuleForEach(c => c.Items).SetValidator(_itemValidator);
        }
    }
}
