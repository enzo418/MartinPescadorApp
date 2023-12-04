using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace FisherTournament.Application.Tournaments.Commands.AddCategory
{
    public class EditCategoryCommandValidator : AbstractValidator<AddCategoryCommand>
    {
        public EditCategoryCommandValidator()
        {
            RuleFor(c => c.TournamentId).NotEmpty();
            RuleFor(c => c.Name).NotEmpty();
        }
    }
}