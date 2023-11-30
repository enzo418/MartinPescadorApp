using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Application.Common.Requests;
using MediatR;

namespace FisherTournament.Application.Competitions.Queries.SearchCompetitions
{
	public record struct SearchCompetitionsQuery(
		int Year,
		int N,
		int Page,
		int PageSize)
		: IRequest<ErrorOr<PagedList<SearchCompetitionsQueryResult>>>, IPagedListQuery;

	public record struct SearchCompetitionsLocationQueryResult(
		string City,
		string State,
		string Country,
		string Place);

	public record struct SearchCompetitionsQueryResult(
		string TournamentName,
		string TournamentId,
		string Id,
		int N,
		DateTime StartDate,
		DateTime? EndDate,
		SearchCompetitionsLocationQueryResult Location);

	public class SearchCompetitionsQueryHandler
		: IRequestHandler<SearchCompetitionsQuery, ErrorOr<PagedList<SearchCompetitionsQueryResult>>>
	{
		private readonly ITournamentFisherDbContext _context;

		public SearchCompetitionsQueryHandler(ITournamentFisherDbContext context)
		{
			_context = context;
		}

		public async Task<ErrorOr<PagedList<SearchCompetitionsQueryResult>>>
			Handle(SearchCompetitionsQuery request, CancellationToken cancellationToken)
		{
			IQueryable<SearchCompetitionsQueryResult> query = _context.Competitions
				.Where(t => t.StartDateTime.Year == request.Year)
				.Where(t => t.N == request.N)
				.Join(_context.Tournaments, c => c.TournamentId, t => t.Id, (c, t) => new { c, t })
				.Select(res => new SearchCompetitionsQueryResult(
					res.t.Name,
					res.t.Id.ToString(),
					res.c.Id.ToString(),
					res.c.N,
					res.c.StartDateTime,
					res.c.EndDateTime,
						new SearchCompetitionsLocationQueryResult(
							res.c.Location.City,
							res.c.Location.State,
							res.c.Location.Country,
							res.c.Location.Place)));

			var competitions = await PagedList<SearchCompetitionsQueryResult>.CreateAsync(
				query,
				request.Page,
				request.PageSize);

			return competitions;
		}
	}
}
