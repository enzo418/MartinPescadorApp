using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Application.Common.Requests;
using MediatR;

namespace FisherTournament.Application.Tournaments.Queries.GetTournamentsSummary
{
	public record struct GetTournamentsSummaryQuery(
		bool? Ended,
		int? TournamentYear,
		int Page,
		int PageSize)
		: IRequest<ErrorOr<PagedList<GetTournamentsSummaryQueryResult>>>, IPagedListQuery;

	public record struct GetTournamentsSummaryCompetitionLocationQueryResult(
		string City,
		string State,
		string Country,
		string Place);

	public record struct GetTournamentsSummaryCompetitionQueryResult(
		int N,
		DateTime StartDate,
		DateTime? EndDate,
		GetTournamentsSummaryCompetitionLocationQueryResult Location);

	public record struct GetTournamentsSummaryQueryResult(
		string Id,
		string Name,
		DateTime StartDate,
		DateTime? EndDate,
		int CompetitionsCount,
		int CategoriesCount,
		int InscriptionsCount
	);

	/// <summary>
	/// Get summary of queried tournaments.
	/// You can filter them by:
	///		- Ended: true/false
	///		- Tournament Year: Year of the start date
	/// </summary>
	public class GetTournamentsSummaryQueryHandler
		: IRequestHandler<GetTournamentsSummaryQuery, ErrorOr<PagedList<GetTournamentsSummaryQueryResult>>>
	{
		private readonly ITournamentFisherDbContext _context;

		public GetTournamentsSummaryQueryHandler(ITournamentFisherDbContext context)
		{
			_context = context;
		}

		public async Task<ErrorOr<PagedList<GetTournamentsSummaryQueryResult>>>
			Handle(GetTournamentsSummaryQuery request, CancellationToken cancellationToken)
		{
			var query = _context.Tournaments.OrderByDescending(t => t.StartDate).AsQueryable();

			if (request.Ended.HasValue)
			{
				query = query.Where(t => t.EndDate.HasValue == request.Ended.Value);
			}

			if (request.TournamentYear.HasValue)
			{
				query = query.Where(t => t.StartDate.Year == request.TournamentYear.Value);
			}

			var tournaments = await PagedList<GetTournamentsSummaryQueryResult>.CreateAsync(
								query.Select(t => new GetTournamentsSummaryQueryResult(
									t.Id.ToString(),
									t.Name,
									t.StartDate,
									t.EndDate,
									t.CompetitionsIds.Count(),
									t.Categories.Count(),
									t.Inscriptions.Count())
								),
								request.Page,
								request.PageSize);

			return tournaments;
		}
	}
}
