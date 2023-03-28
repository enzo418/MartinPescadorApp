using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.ReadModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Tournaments.Queries.GetTournamentLeaderBoard;

public record GetTournamentLeaderBoardQuery(string TournamentId)
  : IRequest<ErrorOr<IEnumerable<TournamentLeaderBoardCategory>>>;

public record struct CompetitionLeaderBoardItem(
    FisherId FisherId,
    string FirstName,
    string LastName,
    int Position,
    int TotalScore
);

public record struct TournamentLeaderBoardCategory(
    CategoryId Id,
    string Name,
    IEnumerable<CompetitionLeaderBoardItem> LeaderBoard
);

public class GetTournamentLeaderBoardQueryHandler
 : IRequestHandler<GetTournamentLeaderBoardQuery, ErrorOr<IEnumerable<TournamentLeaderBoardCategory>>>
{
    private readonly ITournamentFisherDbContext _context;
    private readonly ILeaderBoardRepository _leaderBoardRepository;

    public GetTournamentLeaderBoardQueryHandler(ITournamentFisherDbContext context, ILeaderBoardRepository leaderBoardRepository)
    {
        _context = context;
        _leaderBoardRepository = leaderBoardRepository;
    }

    public async Task<ErrorOr<IEnumerable<TournamentLeaderBoardCategory>>> Handle(
        GetTournamentLeaderBoardQuery request,
        CancellationToken cancellationToken)
    {
        ErrorOr<TournamentId> tournamentId = TournamentId.Create(request.TournamentId);

        if (tournamentId.IsError)
        {
            return Errors.Id.NotValidWithProperty(nameof(request.TournamentId));
        }
        
        var leaderBoard = _leaderBoardRepository.GetTournamentLeaderBoard(tournamentId.Value);
        
        // OPTIMIZE: Cache fisher names instead of a join?
        var fishersNames = await _context.Fishers
            .Where(f => leaderBoard.Select(l => l.FisherId).Contains(f.Id))
            .Join(_context.Users, f => f.UserId, u => u.Id, (f, u) => new { f.Id, u.FirstName, u.LastName })
            .Select(f => new { f.Id, f.FirstName, f.LastName })
            .ToListAsync(cancellationToken);

        // FIXME: we need the categories names!!! category.key is using the id

        var categories = leaderBoard
            .GroupBy(r => r.CategoryId)
            .Select(category => new TournamentLeaderBoardCategory(
                    category.Key,
                    category.First().CategoryId,
                    category.Select(r =>
                    {
                        var fisher = fishersNames.FirstOrDefault(f => f.Id == r.FisherId);
                        
                        return new CompetitionLeaderBoardItem(
                            r.FisherId,
                            fisher?.FirstName ?? string.Empty,
                            fisher?.LastName ?? string.Empty,
                            r.Position,
                            r.TotalScore
                        );
                    })
                )
            );

        return categories.ToList();
    }
}