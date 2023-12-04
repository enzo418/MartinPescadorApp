using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;

namespace FisherTournament.Application.Tournaments.Queries.GetInscription
{
	public record struct GetInscriptionQuery(string TournamentId, string FisherId)
		: IRequest<ErrorOr<GetInscriptionResult>>;

	public record struct GetInscriptionResult(int Number, string FisherId, string CategoryId);

	public class GetInscriptionQueryHandler
		: IRequestHandler<GetInscriptionQuery, ErrorOr<GetInscriptionResult>>
	{
		private readonly ITournamentFisherDbContext _context;

		public GetInscriptionQueryHandler(ITournamentFisherDbContext context)
		{
			_context = context;
		}

		public async Task<ErrorOr<GetInscriptionResult>> Handle(GetInscriptionQuery request, CancellationToken cancellationToken)
		{
			var tournamentId = TournamentId.Create(request.TournamentId);

			if (tournamentId.IsError)
			{
				return Errors.Id.NotValidWithProperty(nameof(request.TournamentId));
			}

			var fisherId = FisherId.Create(request.FisherId);

			if (fisherId.IsError)
			{
				return Errors.Id.NotValidWithProperty(nameof(request.FisherId));
			}

			var tournament = await _context.Tournaments.FindAsync(tournamentId.Value);

			if (tournament is null)
			{
				return Errors.Tournaments.NotFound;
			}

			var insc = tournament.Inscriptions.Where(i => i.FisherId == fisherId.Value).FirstOrDefault();

			if (insc is null)
			{
				return Errors.Tournaments.InscriptionNotFound;
			}

			return new GetInscriptionResult(insc.Number, insc.FisherId.ToString(), insc.CategoryId.ToString());
		}
	}
}
