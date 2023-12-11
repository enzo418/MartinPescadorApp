using ErrorOr;
using FisherTournament.Application.Common.Instrumentation;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.Entities;
using FisherTournament.ReadModels.Models;
using FisherTournament.ReadModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FisherTournament.Application.Competitions.Queries.GetLeaderBoard;

public record struct GetCompetitionLeaderBoardQuery(string CompetitionId)
     : IRequest<ErrorOr<IEnumerable<LeaderBoardCategory>>>;

public record struct LeaderBoardItem(string FisherId, string Name, int Position, int TotalScore, string? TieBreakingReason);

public record struct LeaderBoardCategory(string Name, string Id, IEnumerable<LeaderBoardItem> LeaderBoard);

public record FishersWithName(FisherId Id, string Name);

public class GetCompetitionLeaderBoardQueryHandler
 : IRequestHandler<GetCompetitionLeaderBoardQuery, ErrorOr<IEnumerable<LeaderBoardCategory>>>
{
    private readonly ITournamentFisherDbContext _context;
    private readonly ILeaderBoardRepository _leaderBoardRepository;
    private readonly ApplicationInstrumentation _instrumentation;
    private readonly ILogger<GetCompetitionLeaderBoardQueryHandler> _logger;

    public GetCompetitionLeaderBoardQueryHandler(ITournamentFisherDbContext context,
                                                 ILeaderBoardRepository leaderBoardRepository,
                                                 ApplicationInstrumentation instrumentation,
                                                 ILogger<GetCompetitionLeaderBoardQueryHandler> logger)
    {
        _context = context;
        _leaderBoardRepository = leaderBoardRepository;
        _instrumentation = instrumentation;
        _logger = logger;
    }

    public async Task<ErrorOr<IEnumerable<LeaderBoardCategory>>> Handle(
        GetCompetitionLeaderBoardQuery request,
        CancellationToken cancellationToken)
    {
        ErrorOr<CompetitionId> competitionId = CompetitionId.Create(request.CompetitionId);

        if (competitionId.IsError)
        {
            return Errors.Id.NotValidWithProperty(nameof(request.CompetitionId));
        }

        IEnumerable<LeaderboardCompetitionCategoryItem> leaderBoard;
        using (_instrumentation.ActivitySource.StartActivity("GetLeaderBoard"))
        {
            leaderBoard = _leaderBoardRepository.GetCompetitionLeaderBoard(competitionId.Value);
        }

        // OPTIMIZE: Cache fisher names instead of a join? 
        List<FishersWithName> fishersNames = new();
        using (_instrumentation.ActivitySource.StartActivity("SelectNames"))
        {
            fishersNames = await _context.Fishers
               .Where(f => leaderBoard.Select(l => l.FisherId).Contains(f.Id))
               .Select(f => new FishersWithName(f.Id, f.Name))
               .AsNoTracking()
               .ToListAsync(cancellationToken);
        }

        // OPTIMIZE: Also a very good cache candidate
        IReadOnlyCollection<Category>? tournamentCategories;
        using (_instrumentation.ActivitySource.StartActivity("GetCategories"))
        {
            tournamentCategories = (await _context.Competitions
                .Where(c => c.Id == competitionId.Value)
                .Join(_context.Tournaments, c => c.TournamentId, trn => trn.Id, (c, trn) => trn)
                .Select(t => t.Categories)
                .AsNoTracking()
                .ToListAsync(cancellationToken)
            ).FirstOrDefault();
        }

        if (tournamentCategories is null)
        {
            _logger.LogError("Tournament categories not found for competition {CompetitionId}", competitionId.Value);
            return new List<LeaderBoardCategory>();
        }

        var categories = leaderBoard
            .GroupBy(r => r.CategoryId)
            .Select(category => new LeaderBoardCategory(
                    tournamentCategories.First(c => c.Id == category.Key)?.Name ?? category.Key,
                    category.Key,
                    category.Select(r =>
                    {
                        var fisher = fishersNames.FirstOrDefault(f => f.Id == r.FisherId);

                        return new LeaderBoardItem(
                            r.FisherId.ToString(),
                            fisher?.Name ?? string.Empty,
                            r.Position,
                            r.Score,
                            r.TieBreakingReason
                        );
                    })
                )
            ).ToList();

        return categories;
    }
}