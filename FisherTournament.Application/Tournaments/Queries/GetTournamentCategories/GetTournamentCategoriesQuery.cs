using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;

namespace FisherTournament.Application.Tournaments.Queries.GetTournamentCategories
{
    public record GetTournamentCategoriesQuery(string TournamentId)
    : IRequest<ErrorOr<IEnumerable<GetTournamentCategoriesQueryResult>>>;

    public record struct GetTournamentCategoriesQueryResult(
        string Id,
        string Name
    );

    public class GetTournamentCategoriesQueryHandler
         : IRequestHandler<GetTournamentCategoriesQuery, ErrorOr<IEnumerable<GetTournamentCategoriesQueryResult>>>
    {
        private readonly ITournamentFisherDbContext _context;

        public GetTournamentCategoriesQueryHandler(ITournamentFisherDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<IEnumerable<GetTournamentCategoriesQueryResult>>>
            Handle(GetTournamentCategoriesQuery request, CancellationToken cancellationToken)
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

            var categories = tournament.Categories//.Where(c => c.Name != Tournament.GeneralCategoryName)
                                                  .Select(c => new GetTournamentCategoriesQueryResult(c.Id.ToString(), c.Name));

            return categories.ToList();
        }
    }
}
