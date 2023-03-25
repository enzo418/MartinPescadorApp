using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.TournamentAggregate.Entities;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;

namespace FisherTournament.Application.Tournaments.Commands.AddCategory
{
    public record struct AddCategoryCommand(string TournamentId, string Name)
        : IRequest<ErrorOr<AddCategoryCommandResponse>>;

    public record struct AddCategoryCommandResponse(string Id, string Name);

    public sealed class AddCategoryCommandHandler
     : IRequestHandler<AddCategoryCommand, ErrorOr<AddCategoryCommandResponse>>
    {
        private readonly ITournamentFisherDbContext _context;

        public AddCategoryCommandHandler(ITournamentFisherDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<AddCategoryCommandResponse>> Handle(
            AddCategoryCommand request,
            CancellationToken cancellationToken)
        {
            ErrorOr<TournamentId> tournamentId = TournamentId.Create(request.TournamentId);

            if (tournamentId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(request.TournamentId));
            }

            var tournament = _context.Tournaments.FirstOrDefault(t => t.Id == tournamentId.Value);

            if (tournament is null)
            {
                return Errors.Tournaments.NotFound;
            }

            Category category = Category.Create(request.Name);

            ErrorOr<Category> result = tournament.AddCategory(category);

            if (result.IsError)
            {
                return result.Errors;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new AddCategoryCommandResponse(category.Id.ToString(), category.Name);
        }
    }
}