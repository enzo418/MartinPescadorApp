using FisherTournament.Application.Common.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Competitions.Queries.GetLeaderBoard;

public record struct GetLeaderBoardQuery(Guid CompetitionId) : IRequest<IEnumerable<LeaderBoardItemDto>>;

public record struct LeaderBoardItemDto(Guid FisherId, string FirstName, string LastName, int TotalScore);

public class GetLeaderBoardQueryHandler
 : IRequestHandler<GetLeaderBoardQuery, IEnumerable<LeaderBoardItemDto>>
{
    private readonly ITournamentFisherDbContext _context;

    public GetLeaderBoardQueryHandler(ITournamentFisherDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LeaderBoardItemDto>> Handle(
        GetLeaderBoardQuery request,
        CancellationToken cancellationToken)
    {
        // Select CompetitionParticipations join with Fisher to get their names
        // and then group by FisherId and sum the scores.
        var query = from c in _context.Competitions
                    where c.Id == request.CompetitionId
                    from p in c.Participations
                    join f in _context.Fishers on p.FisherId equals f.Id
                    join u in _context.Users on f.UserId equals u.Id
                    orderby p.TotalScore descending
                    select new LeaderBoardItemDto(f.Id, u.FirstName, u.LastName, p.TotalScore);

        return await query.ToListAsync(cancellationToken);
    }
}