using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Competitions.Queries.GetTournamentCompetitions
{
	public record struct GetTournamentCompetitionsQuery(string TournamentId)
	: IRequest<ErrorOr<List<GetTournamentCompetitionsQueryResult>>>;

	public record struct GetTournamentCompetitionsLocationQueryResult(string City, string State, string Country, string Place);

	public record struct GetTournamentCompetitionsQueryResult(
		int N,
		DateTime StartDate,
		DateTime? EndDate,
		GetTournamentCompetitionsLocationQueryResult Location
	);

	public class GetTournamentCompetitionsQueryHandler
		 : IRequestHandler<GetTournamentCompetitionsQuery, ErrorOr<List<GetTournamentCompetitionsQueryResult>>>
	{
		private readonly ITournamentFisherDbContext _context;

		public GetTournamentCompetitionsQueryHandler(ITournamentFisherDbContext context)
		{
			_context = context;
		}

		public async Task<ErrorOr<List<GetTournamentCompetitionsQueryResult>>>
			Handle(GetTournamentCompetitionsQuery request, CancellationToken cancellationToken)
		{
			var tournamentId = TournamentId.Create(request.TournamentId);

			if (tournamentId.IsError)
			{
				return Errors.Id.NotValidWithProperty(nameof(request.TournamentId));
			}

			var tournament = await _context.Tournaments
				.Where(t => t.Id == tournamentId.Value)
				.AsNoTracking()
				.FirstOrDefaultAsync(cancellationToken);

			if (tournament == null)
			{
				return Errors.Tournaments.NotFound;
			}

			var competitions = await _context.Competitions.Where(c => c.TournamentId == tournamentId.Value)
				.Select(c => new GetTournamentCompetitionsQueryResult(
					c.N,
					c.StartDateTime,
					c.EndDateTime,
					new GetTournamentCompetitionsLocationQueryResult(c.Location.City, c.Location.State, c.Location.Country, c.Location.Place)))
				.ToListAsync(cancellationToken);

			return competitions;
		}
	}
}
