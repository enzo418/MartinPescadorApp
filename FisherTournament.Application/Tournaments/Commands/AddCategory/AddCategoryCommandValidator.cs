using FluentValidation;

namespace FisherTournament.Application.Tournaments.Commands.AddCategory
{
	public class AddCategoryCommandValidator : AbstractValidator<AddCategoryCommand>
	{
		public AddCategoryCommandValidator()
		{
			RuleFor(c => c.TournamentId).NotEmpty();
			RuleFor(c => c.Name).NotEmpty();
		}
	}
}