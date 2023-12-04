using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;

namespace FisherTournament.Application.Tournaments.Commands.DeleteCategory
{
    public record struct DeleteCategoryCommand(string TournamentId, string CategoryId)
        : IRequest<ErrorOr<Deleted>>;

    public class DeleteCategoryCommandHandler
        : IRequestHandler<DeleteCategoryCommand, ErrorOr<Deleted>>
    {
        private readonly ITournamentFisherDbContext _context;

        public DeleteCategoryCommandHandler(ITournamentFisherDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<Deleted>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var tournamentId = TournamentId.Create(request.TournamentId);

            if (tournamentId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(request.TournamentId));
            }

            var tournament = await _context.Tournaments
                .FindAsync(new object[] { tournamentId.Value }, cancellationToken: cancellationToken);

            if (tournament == null)
            {
                return Errors.Tournaments.NotFound;
            }

            var categoryId = CategoryId.Create(request.CategoryId);

            if (categoryId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(request.CategoryId));
            }

            if (tournament.DeleteCategory(categoryId.Value) is var res && res.IsError)
            {
                return res.Errors;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}
