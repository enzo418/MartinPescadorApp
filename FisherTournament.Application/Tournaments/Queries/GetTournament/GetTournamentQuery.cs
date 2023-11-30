using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Tournaments.Queries.GetTournament
{
	public record GetTournamentQuery(string TournamentId)
	: IRequest<ErrorOr<GetTournamentResultQueryResult>>;

	public record struct GetTournamentResultQueryResult(
		string Name,
		DateTime StartDate,
		DateTime? EndDate
	);

	public class GetTournamentQueryHandler
		 : IRequestHandler<GetTournamentQuery, ErrorOr<GetTournamentResultQueryResult>>
	{
		private readonly ITournamentFisherDbContext _context;

		public GetTournamentQueryHandler(ITournamentFisherDbContext context)
		{
			_context = context;
		}

		public async Task<ErrorOr<GetTournamentResultQueryResult>>
			Handle(GetTournamentQuery request, CancellationToken cancellationToken)
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

			return new GetTournamentResultQueryResult(
				tournament.Name,
				tournament.StartDate,
				tournament.EndDate);
		}
	}
}
