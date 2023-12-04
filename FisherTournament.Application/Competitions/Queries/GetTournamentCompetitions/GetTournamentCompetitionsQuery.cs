using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Application.Common.Resources;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Competitions.Queries.GetTournamentCompetitions
{
	public record struct GetTournamentCompetitionsQuery(string TournamentId)
	: IRequest<ErrorOr<List<GetTournamentCompetitionsQueryResult>>>;

	public record struct GetTournamentCompetitionsQueryResult(
		string Id,
		int N,
		DateTime StartDate,
		DateTime? EndDate,
		CompetitionLocationResource Location
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

			var tournamentExists = await _context.Tournaments
					.AnyAsync(t => t.Id == tournamentId.Value, cancellationToken: cancellationToken);

			if (!tournamentExists)
			{
				return Errors.Tournaments.NotFound;
			}

			var competitions = await _context.Competitions.Where(c => c.TournamentId == tournamentId.Value)
				.Select(c => new GetTournamentCompetitionsQueryResult(
					c.Id.ToString(),
					c.N,
					c.StartDateTime,
					c.EndDateTime,
					new CompetitionLocationResource(c.Location.City, c.Location.State, c.Location.Country, c.Location.Place)))
				.ToListAsync(cancellationToken);

			return competitions;
		}
	}
}
