using ErrorOr;
using FisherTournament.Application.Common.Instrumentation;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.TournamentAggregate.Entities;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.ReadModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FisherTournament.Application.Tournaments.Queries.GetTournamentLeaderBoard;

public record GetTournamentLeaderBoardQuery(string TournamentId)
  : IRequest<ErrorOr<IEnumerable<TournamentLeaderBoardCategory>>>;

public record struct TournamentLeaderBoardItem(
    string FisherId,
    string Name,
    int Position,
    List<int> CompetitionPositions
);

public record struct TournamentLeaderBoardCategory(
    string Id,
    string Name,
    IEnumerable<TournamentLeaderBoardItem> LeaderBoard
);

public class GetTournamentLeaderBoardQueryHandler
 : IRequestHandler<GetTournamentLeaderBoardQuery, ErrorOr<IEnumerable<TournamentLeaderBoardCategory>>>
{
    private readonly ITournamentFisherDbContext _context;
    private readonly ILeaderBoardRepository _leaderBoardRepository;
    private readonly ApplicationInstrumentation _instrumentation;
    private readonly ILogger<GetTournamentLeaderBoardQueryHandler> _logger;

    public GetTournamentLeaderBoardQueryHandler(ITournamentFisherDbContext context,
                                                ILeaderBoardRepository leaderBoardRepository,
                                                ILogger<GetTournamentLeaderBoardQueryHandler> logger,
                                                ApplicationInstrumentation instrumentation)
    {
        _context = context;
        _leaderBoardRepository = leaderBoardRepository;
        _logger = logger;
        _instrumentation = instrumentation;
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
            .Select(f => new { f.Id, f.Name })
            .ToListAsync(cancellationToken);

        // OPTIMIZE: Also a very good cache candidate
        IReadOnlyCollection<Category>? tournamentCategories;
        using (_instrumentation.ActivitySource.StartActivity("GetCategories"))
        {
            tournamentCategories = (await _context.Tournaments
                .Where(t => t.Id == tournamentId.Value)
                .Select(t => t.Categories)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
            ).FirstOrDefault();
        }

        if (tournamentCategories is null)
        {
            _logger.LogError("Categories of tournament {TournamentId} could not be found", tournamentId.Value);
            return new List<TournamentLeaderBoardCategory>();
        }

        var categories = leaderBoard
            .GroupBy(r => r.CategoryId)
            .Select(category => new TournamentLeaderBoardCategory(
                    category.Key,
                    tournamentCategories.First(c => c.Id == category.Key)?.Name ?? category.Key,
                    category.Select(r =>
                    {
                        var fisher = fishersNames.FirstOrDefault(f => f.Id == r.FisherId);

                        return new TournamentLeaderBoardItem(
                            r.FisherId.ToString(),
                            fisher?.Name ?? string.Empty,
                            r.Position,
                            r.Positions
                        );
                    })
                )
            );

        // Calculate "General" category, which is the sum of all categories. Whoever has the lowest sum wins.
        int position = 0;
        var generalCategory = new TournamentLeaderBoardCategory(
                                    "General",
                                    "General",
                                    categories.SelectMany(c => c.LeaderBoard)
                                        .OrderBy(l => l.CompetitionPositions.Sum())
                                        .ThenBy(l => l.FisherId)
                                        .Select(l => new TournamentLeaderBoardItem(
                                            l.FisherId,
                                            l.Name,
                                            ++position,
                                            l.CompetitionPositions
                                        ))
                                        .ToList()
                                    );

        return new List<TournamentLeaderBoardCategory>(categories) { generalCategory };
    }
}