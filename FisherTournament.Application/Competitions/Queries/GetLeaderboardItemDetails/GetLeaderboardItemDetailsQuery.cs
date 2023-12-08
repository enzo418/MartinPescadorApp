using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Competitions.Queries.GetLeaderboardItemDetails
{
    public record struct GetLeaderboardItemDetailsQuery(string CompetitionId, string FisherId)
        : IRequest<ErrorOr<GetLeaderboardItemDetailsQueryResult>>;

    public record struct FishCaughtResult(int Score, DateTime DateTime);

    public record struct GetLeaderboardItemDetailsQueryResult(string CompetitionId, string FisherId, int TotalScore, IEnumerable<FishCaughtResult> Caught);

    public class GetLeaderboardItemDetailsQueryHandler
        : IRequestHandler<GetLeaderboardItemDetailsQuery, ErrorOr<GetLeaderboardItemDetailsQueryResult>>
    {
        private readonly ITournamentFisherDbContext _context;

        public GetLeaderboardItemDetailsQueryHandler(ITournamentFisherDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<GetLeaderboardItemDetailsQueryResult>> Handle(GetLeaderboardItemDetailsQuery request, CancellationToken cancellationToken)
        {
            var competitionId = CompetitionId.Create(request.CompetitionId);

            if (competitionId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(request.CompetitionId));
            }

            var fisherId = FisherId.Create(request.FisherId);

            if (fisherId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(request.FisherId));
            }

            var result = await (
                from c in _context.Competitions
                from p in c.Participations
                from f in p.FishCaught
                where c.Id == competitionId.Value
                where p.FisherId == fisherId.Value
                select new
                {
                    CompetitionId = c.Id,
                    FisherId = p.FisherId,
                    TotalScore = p.FishCaught.Sum(x => x.Score),
                    Caught = p.FishCaught.Select(x => new FishCaughtResult(x.Score, x.DateTime))
                }
            ).FirstOrDefaultAsync(cancellationToken);

            if (result == null)
            {
                return Errors.Fishers.FisherHasNoParticipationRegistered;
            }

            return new GetLeaderboardItemDetailsQueryResult(
                    result.CompetitionId.ToString(),
                    result.FisherId.ToString(),
                    result.TotalScore,
                    result.Caught.Select(x => new FishCaughtResult(x.Score, x.DateTime))
            );
        }
    }
}
