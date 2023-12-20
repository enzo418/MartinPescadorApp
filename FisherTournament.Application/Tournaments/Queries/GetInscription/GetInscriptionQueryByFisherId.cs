using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Domain.TournamentAggregate.Entities;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;

namespace FisherTournament.Application.Tournaments.Queries.GetInscription
{
    public record struct GetInscriptionQueryByFisherId(string TournamentId, string FisherId)
        : IRequest<ErrorOr<GetInscriptionResult>>;

    public record struct GetInscriptionQueryByInscriptionNumber(string TournamentId, int InscriptionNumber)
        : IRequest<ErrorOr<GetInscriptionResult>>;

    public record struct GetInscriptionResult(int Number, string FisherId, string CategoryId);

    public class GetInscriptionQueryHandler
        : IRequestHandler<GetInscriptionQueryByFisherId, ErrorOr<GetInscriptionResult>>,
        IRequestHandler<GetInscriptionQueryByInscriptionNumber, ErrorOr<GetInscriptionResult>>
    {
        private readonly ITournamentFisherDbContext _context;

        public GetInscriptionQueryHandler(ITournamentFisherDbContext context)
        {
            _context = context;
        }

        private async Task<ErrorOr<Tournament>> GetTournament(string tournamentId)
        {
            var pTournamentId = TournamentId.Create(tournamentId);

            if (pTournamentId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(tournamentId));
            }

            var tournament = await _context.Tournaments.FindAsync(pTournamentId.Value);

            if (tournament is null)
            {
                return Errors.Tournaments.NotFound;
            }

            return tournament;
        }

        private static ErrorOr<GetInscriptionResult> ReturnInscription(TournamentInscription? insc)
        {
            if (insc is null)
            {
                return Errors.Tournaments.InscriptionNotFound;
            }

            return new GetInscriptionResult(insc.Number, insc.FisherId.ToString(), insc.CategoryId.ToString());
        }

        public async Task<ErrorOr<GetInscriptionResult>> Handle(GetInscriptionQueryByFisherId request, CancellationToken cancellationToken)
        {
            var tournament = await GetTournament(request.TournamentId);

            if (tournament.IsError) return tournament.Errors;

            var fisherId = FisherId.Create(request.FisherId);

            if (fisherId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(request.FisherId));
            }

            var insc = tournament.Value.Inscriptions.Where(i => i.FisherId == fisherId.Value).FirstOrDefault();

            return ReturnInscription(insc);
        }

        public async Task<ErrorOr<GetInscriptionResult>> Handle(GetInscriptionQueryByInscriptionNumber request, CancellationToken cancellationToken)
        {
            var tournament = await GetTournament(request.TournamentId);

            if (tournament.IsError) return tournament.Errors;

            var insc = tournament.Value.Inscriptions.Where(i => i.Number == request.InscriptionNumber).FirstOrDefault();

            return ReturnInscription(insc);
        }
    }
}
