using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FisherTournament.Domain.Common.Errors;

namespace FisherTournament.Application.Competitions.Queries.GetLeaderBoard;

public record struct GetLeaderBoardQuery(string CompetitionId)
     : IRequest<ErrorOr<IEnumerable<LeaderBoardItemDto>>>;

public record struct LeaderBoardItemDto(FisherId FisherId, string FirstName, string LastName, int TotalScore);

public class GetLeaderBoardQueryHandler
 : IRequestHandler<GetLeaderBoardQuery, ErrorOr<IEnumerable<LeaderBoardItemDto>>>
{
    private readonly ITournamentFisherDbContext _context;

    public GetLeaderBoardQueryHandler(ITournamentFisherDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<IEnumerable<LeaderBoardItemDto>>> Handle(
        GetLeaderBoardQuery request,
        CancellationToken cancellationToken)
    {
        ErrorOr<CompetitionId> competitionId = CompetitionId.Create(request.CompetitionId);

        if (competitionId.IsError)
        {
            return Errors.Id.NotValidWithProperty(nameof(request.CompetitionId));
        }

        // Select CompetitionParticipations join with Fisher to get their names
        // and then group by FisherId and sum the scores.
        var query = from c in _context.Competitions
                    where c.Id == competitionId.Value
                    from p in c.Participations
                    join f in _context.Fishers on p.FisherId equals f.Id
                    join u in _context.Users on f.UserId equals u.Id
                    orderby p.TotalScore descending
                    select new LeaderBoardItemDto(f.Id, u.FirstName, u.LastName, p.TotalScore);

        return await query.ToListAsync(cancellationToken);
    }
}