using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FisherTournament.Domain.Common.Errors;

namespace FisherTournament.Application.Competitions.Queries.GetLeaderBoard;

public record struct GetLeaderBoardQuery(string CompetitionId)
     : IRequest<ErrorOr<IEnumerable<LeaderBoardCategory>>>;

public record struct LeaderBoardItem(FisherId FisherId, string FirstName, string LastName, int TotalScore);

public record struct LeaderBoardCategory(string Name, string Id, IEnumerable<LeaderBoardItem> LeaderBoard);

public class GetLeaderBoardQueryHandler
 : IRequestHandler<GetLeaderBoardQuery, ErrorOr<IEnumerable<LeaderBoardCategory>>>
{
    private readonly ITournamentFisherDbContext _context;

    public GetLeaderBoardQueryHandler(ITournamentFisherDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<IEnumerable<LeaderBoardCategory>>> Handle(
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
        var query = from comp in _context.Competitions

                    where comp.Id == competitionId.Value

                    from participation in comp.Participations
                    from tn in _context.Tournaments
                    from category in tn.Categories
                    from inscription in tn.Inscriptions

                    where inscription.FisherId == participation.FisherId
                    where inscription.CategoryId == category.Id
                    where tn.Id == comp.TournamentId

                    join fisher in _context.Fishers on participation.FisherId equals fisher.Id
                    join user in _context.Users on fisher.UserId equals user.Id
                    orderby category.Name, participation.TotalScore descending
                    select new
                    {
                        CategoryName = category.Name,
                        CategoryId = category.Id,
                        FisherId = fisher.Id,
                        user.FirstName,
                        user.LastName,
                        participation.TotalScore
                    };

        var result = await query.ToListAsync(cancellationToken);

        var categories = result
            .GroupBy(r => r.CategoryName)
            .Select(category => new LeaderBoardCategory(
                category.Key,
                category.First().CategoryId,
                category.Select(r => new LeaderBoardItem(
                    r.FisherId,
                    r.FirstName,
                    r.LastName,
                    r.TotalScore
                ))
            ));

        return new List<LeaderBoardCategory>(categories);
    }
}