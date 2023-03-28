using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.ReadModels.Models;
using FisherTournament.ReadModels.Persistence;

namespace FisherTournament.Application.Competitions.Queries.GetLeaderBoard;

public record struct GetLeaderBoardQuery(string CompetitionId)
     : IRequest<ErrorOr<IEnumerable<LeaderBoardCategory>>>;

public record struct LeaderBoardItem(FisherId FisherId, string FirstName, string LastName, int Position, int TotalScore);

public record struct LeaderBoardCategory(string Name, string Id, IEnumerable<LeaderBoardItem> LeaderBoard);

public class GetLeaderBoardQueryHandler
 : IRequestHandler<GetLeaderBoardQuery, ErrorOr<IEnumerable<LeaderBoardCategory>>>
{
    private readonly ITournamentFisherDbContext _context;
    private readonly ILeaderBoardRepository _leaderBoardRepository;

    public GetLeaderBoardQueryHandler(ITournamentFisherDbContext context, ILeaderBoardRepository leaderBoardRepository)
    {
        _context = context;
        _leaderBoardRepository = leaderBoardRepository;
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

        IEnumerable<LeaderboardCompetitionCategoryItem> leaderBoard =
            _leaderBoardRepository.GetCompetitionLeaderBoard(competitionId.Value);
        
        // OPTIMIZE: Cache fisher names instead of a join? 
        var fishersNames = await _context.Fishers
            .Where(f => leaderBoard.Select(l => l.FisherId).Contains(f.Id))
            .Join(_context.Users, f => f.UserId, u => u.Id, (f, u) => new { f.Id, u.FirstName, u.LastName })
            .Select(f => new { f.Id, f.FirstName, f.LastName })
            .ToListAsync(cancellationToken);
        
        // FIXME: we need the categories names!!! category.key is using the id
        
        var categories = leaderBoard
            .GroupBy(r => r.CategoryId)
            .Select(category => new LeaderBoardCategory(
                    category.Key,
                    category.First().CategoryId,
                    category.Select(r =>
                    {
                        var fisher = fishersNames.FirstOrDefault(f => f.Id == r.FisherId);
                        
                        return new LeaderBoardItem(
                            r.FisherId,
                            fisher?.FirstName ?? string.Empty,
                            fisher?.LastName ?? string.Empty,
                            r.Position,
                            r.Score
                        );
                    })
                )
            );

        return new List<LeaderBoardCategory>(categories);
    }
}