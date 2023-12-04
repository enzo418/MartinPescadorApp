using FluentValidation;

namespace FisherTournament.Application.Tournaments.Commands.EditCategory
{
    public class EditCategoryCommandValidator : AbstractValidator<EditCategoryCommand>
    {
        public EditCategoryCommandValidator()
        {
            RuleFor(c => c.TournamentId).NotEmpty();
            RuleFor(c => c.CategoryId).NotEmpty();
            RuleFor(c => c.Name).NotEmpty();
        }
    }
}