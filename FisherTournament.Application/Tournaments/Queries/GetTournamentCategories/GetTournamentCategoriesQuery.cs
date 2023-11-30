using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;

namespace FisherTournament.Application.Tournaments.Queries.GetTournamentCategories
{
	public record GetTournamentCategoriesQuery(string TournamentId)
	: IRequest<ErrorOr<IEnumerable<GetTournamentCategoriesResultQueryResult>>>;

	public record struct GetTournamentCategoriesResultQueryResult(
		string Name
	);

	public class GetTournamentCategoriesQueryHandler
		 : IRequestHandler<GetTournamentCategoriesQuery, ErrorOr<IEnumerable<GetTournamentCategoriesResultQueryResult>>>
	{
		private readonly ITournamentFisherDbContext _context;

		public GetTournamentCategoriesQueryHandler(ITournamentFisherDbContext context)
		{
			_context = context;
		}

		public async Task<ErrorOr<IEnumerable<GetTournamentCategoriesResultQueryResult>>>
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

			var categories = tournament.Categories.Select(
				c => new GetTournamentCategoriesResultQueryResult(c.Name));

			return categories.ToList();
		}
	}
}
