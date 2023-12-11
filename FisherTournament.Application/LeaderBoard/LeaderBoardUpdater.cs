using FisherTournament.Application.Common.Instrumentation;
using FisherTournament.Application.Common.Metrics;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.ReadModels.Models;
using FisherTournament.ReadModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FisherTournament.Application.LeaderBoard;


public class LeaderBoardUpdater : ILeaderBoardUpdater
{
    private readonly ApplicationInstrumentation _instrumentation;
    private readonly ITournamentFisherDbContext _context;
    private readonly ILogger<LeaderBoardUpdater> _logger;
    private readonly IReadModelsUnitOfWork _readModelsUnitOfWork;
    private readonly IEnumerable<ILeaderboardNotificationClient> _leaderboardNotificationClients;

    public LeaderBoardUpdater(
        ApplicationInstrumentation instrumentation,
        ITournamentFisherDbContext context,
        ILogger<LeaderBoardUpdater> logger,
        IReadModelsUnitOfWork readModelsUnitOfWork,
        IEnumerable<ILeaderboardNotificationClient> leaderboardNotificationClients)
    {
        _instrumentation = instrumentation;
        _context = context;
        _logger = logger;
        _readModelsUnitOfWork = readModelsUnitOfWork;
        _leaderboardNotificationClients = leaderboardNotificationClients;
    }

    /// <summary>
    /// Updates the leader boards of a tournament.
    /// </summary>
    /// <param name="tournamentId"></param>
    /// <param name="categoryId">will update only the leader board of that category</param>
    /// <param name="competitionsIds">will update only the leader board of those competitions</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task UpdateLeaderBoard(TournamentId tournamentId,
                                        CategoryId categoryId,
                                        IEnumerable<CompetitionId>? competitionsIds = null,
                                        CancellationToken cancellationToken = default)
    {
        using var leaderBoardTimer = ApplicationMetrics.LeaderboardMetrics.LeaderboardUpdate.Time();
        using var activity = _instrumentation.ActivitySource.StartActivity("UpdateLeaderBoard");

        leaderBoardTimer.AddTag("total-competitions-to-update", competitionsIds?.Count() ?? 0);
        activity?.AddTag("total-competitions-to-update", competitionsIds?.Count() ?? 0);

        if (competitionsIds == null || !competitionsIds.Any())
        {
            competitionsIds = await _context.Competitions
                .Where(x => x.TournamentId == tournamentId)
                .Select(x => x.Id)
                .ToListAsync(cancellationToken);
        }

        var generalCategoryId = await _context.Tournaments
            .Where(t => t.Id == tournamentId)
            .SelectMany(i => i.Categories)
            .Where(c => c.Name == Tournament.GeneralCategoryName)
            .Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);

        int fishersInTournament = await _context.Tournaments
            .Where(t => t.Id == tournamentId)
            .SelectMany(i => i.Inscriptions)
            .CountAsync(cancellationToken);

        if (generalCategoryId is null)
        {
            _logger.LogError("General category not found for tournament {TournamentId}", tournamentId);
            throw new InvalidOperationException($"Unmet domain Rule: General category not found for tournament {tournamentId}");
        }

        foreach (var competitionId in competitionsIds)
        {
            await UpdateCompetitionLeaderBoard(competitionId, categoryId, generalCategoryId, fishersInTournament, cancellationToken);
        }

        await UpdateTournamentLeaderBoard(tournamentId, categoryId, generalCategoryId, cancellationToken);

        // Run notification in parallel
        var competitionsIdsString = competitionsIds.Select(x => x.ToString());
        var tasks = _leaderboardNotificationClients.Select(client => client.OnLeaderboardUpdated(
            tournamentId.ToString(),
            categoryId.ToString(),
            competitionsIdsString));

        await Task.WhenAll(tasks);
    }

    private async Task UpdateCompetitionLeaderBoard(CompetitionId competitionId, CategoryId categoryId, CategoryId generalCategory, int fishersInTournament, CancellationToken cancellationToken = default)
    {
        await UpdateCompetitionLeaderBoard(competitionId, categoryId, false, fishersInTournament, cancellationToken);

        await UpdateCompetitionLeaderBoard(competitionId, generalCategory, true, fishersInTournament, cancellationToken);
    }

    private async Task UpdateCompetitionLeaderBoard(CompetitionId competitionId, CategoryId categoryId, bool isGeneralCategory, int fishersInTournament, CancellationToken cancellationToken = default)
    {
        using var _ = ApplicationMetrics.LeaderboardMetrics.LeaderboardUpdate.Time(new Tag("function", "competition-update"));
        using var ___ = _instrumentation.ActivitySource.StartActivity("UpdateCompetitionLeaderBoard");

        _logger.LogInformation("Updating competition {CompetitionId} leaderboard for category {CategoryId}",
                                competitionId, categoryId);

        // 2. Get each fisher and score from the this category
        // TODO: This query screams for a refactor -> This is a good candidate for a view or repository
        List<FisherWithScore>? fishersFromCategoryWithScore;

        if (isGeneralCategory)
        {
            fishersFromCategoryWithScore = await (
                    from c in _context.Competitions
                    where c.Id == competitionId

                    join t in _context.Tournaments on c.TournamentId equals t.Id
                    from insc in t.Inscriptions

                        // left join - if the fisher is not in the competition, p is null
                    from p in c.Participations.Where(p => p.FisherId == insc.FisherId).DefaultIfEmpty()

                    join f in _context.Fishers on insc.FisherId equals f.Id

                    select new FisherWithScore
                    {
                        FisherId = f.Id,
                        Score = p == null ? 0 : p.TotalScore,
                        LargerPiece = p == null ? 0 : p.FishCaught.Max(piece => piece.Score)
                    }
                ).ToListAsync(cancellationToken);
        } else
        {
            fishersFromCategoryWithScore = await (
                    from c in _context.Competitions
                    where c.Id == competitionId

                    join t in _context.Tournaments on c.TournamentId equals t.Id
                    from insc in t.Inscriptions

                    where insc.CategoryId == categoryId //t.Inscriptions.First(i => i.FisherId == fisherId).CategoryId

                    // left join - if the fisher is not in the competition, p is null
                    from p in c.Participations.Where(p => p.FisherId == insc.FisherId).DefaultIfEmpty()

                    join f in _context.Fishers on insc.FisherId equals f.Id

                    select new FisherWithScore
                    {
                        FisherId = f.Id,
                        Score = p == null ? 0 : p.TotalScore,
                        LargerPiece = p == null ? 0 : p.FishCaught.Max(piece => piece.Score)
                    }
                ).ToListAsync(cancellationToken);
        }

        // 3. Sort the fishers by descending score and break tie

        // first let's prepare for the worts case scenario
        var fishersToLoadCaughtFish = fishersFromCategoryWithScore
                .GroupBy(f => new { f.Score, f.LargerPiece })
                .Where(group => group.Key.Score > 0) // Filter out those that did not catch any fish
                .Where(group => group.Count() > 1) // Only groups with more than 1 fisher
                .Select(group => group.ToList());

        // 3.1 Load the caught fish for the fishers that have the same score and larger piece
        if (fishersToLoadCaughtFish.Any())
        {
            // load all the caught fish
            Dictionary<FisherId, IEnumerable<FishCaught>> fisherCaughtFish = new();
            var flatten = fishersToLoadCaughtFish.SelectMany(d => d);
            foreach (var fisher in flatten)
            {
                fisherCaughtFish[fisher.FisherId] = (
                    from c in _context.Competitions
                    where c.Id == competitionId

                    // left join - if the fisher is not in the competition, p is null
                    from p in c.Participations.Where(p => p.FisherId == fisher.FisherId).DefaultIfEmpty()

                    select p.FishCaught
                ).AsNoTracking().ToList().SelectMany(p => p);
            }

            BreakTieByFirstLargerPiece(fishersToBreakTie: fishersToLoadCaughtFish, fisherCaughtFish);
        }

        int comparison(FisherWithScore x, FisherWithScore y)
        {
            // asc -> x to y
            // desc -> y to x
            if (x.Score != y.Score)
            {
                return y.Score.CompareTo(x.Score);
            } else if (x.LargerPiece != y.LargerPiece)
            {
                x.TieBreakingReason = TieBreakingReasonGenerator.ByLargerPiece(x.LargerPiece);
                y.TieBreakingReason = TieBreakingReasonGenerator.ByLargerPiece(y.LargerPiece);
                return y.LargerPiece.CompareTo(x.LargerPiece);
            } else if (x.TieDiscriminator != y.TieDiscriminator)
            {
                // TieBreakingReason is already set by the previous call to BreakTieByFirstLargerPiece
                return y.TieDiscriminator.CompareTo(x.TieDiscriminator);
            } else if (x.Score != 0) // absent fishers are always last with the same position
            {
                x.TieBreakingReason = TieBreakingReasonGenerator.ByDefault();
                y.TieBreakingReason = TieBreakingReasonGenerator.ByDefault();
            }

            return y.FisherId.Value.CompareTo(x.FisherId.Value);
        }

        fishersFromCategoryWithScore.Sort(comparison);

        /*fishersFromCategoryWithScore = fishersFromCategoryWithScore
                .OrderByDescending(f => f.Score)
                .ThenByDescending(f => f.LargerPiece)
                .ThenByDescending(f => f.TieDiscriminator)
                .ThenByDescending(f => f.FisherId.Value)
                .ToList();*/

        // 4. Get the fishers positions respecting the system rules
        var leaderBoardRepository = _readModelsUnitOfWork.LeaderBoardRepository;

        var previousCategoryLeaderBoard = leaderBoardRepository
                                                .GetCompetitionCategoryLeaderBoard(competitionId,
                                                                                   categoryId);

        int currentPosition = 1;

        _logger.LogInformation($"Will update {fishersFromCategoryWithScore.Count} fishers");

        foreach (var fisher in fishersFromCategoryWithScore)
        {
            if (isGeneralCategory && fisher.Score <= 0)
            {
                currentPosition = fishersInTournament + 1;
            }

            UpdateFisherPositionOnCompetition(competitionId,
                                              categoryId,
                                              leaderBoardRepository,
                                              previousCategoryLeaderBoard,
                                              newPosition: currentPosition,
                                              newScore: fisher.Score,
                                              fisherId: fisher.FisherId,
                                              tieBreakingReason: fisher.TieBreakingReason);

            if (fisher.Score > 0) // the position only is repeated for absent or those that didn't catch any fish
            {
                currentPosition++;
            }
        }

        _readModelsUnitOfWork.Commit();
    }

    /// <summary>
    /// This function generates a "TieDiscriminator" for each fisher that has the same score and larger piece.
    /// Break the tie by "go to the first catch of both, the bigger one wins, if they are the same, go to the second catch, and so on."
    /// </summary>
    /// <param name="fishersToBreakTie"></param>
    /// <param name="fisherCaughtFish"></param>
    private void BreakTieByFirstLargerPiece(IEnumerable<List<FisherWithScore>> fishersToBreakTie,
                                                   Dictionary<FisherId, IEnumerable<FishCaught>> fisherCaughtFish)
    {
        using var _ = _instrumentation.ActivitySource.StartActivity("BreakTieByFirstLargerPiece");

        foreach (var fishers in fishersToBreakTie)
        {
            BreakTieByLargestNthPiece(fishers, fisherCaughtFish);
        }
    }

    private void BreakTieByLargestNthPiece(
        List<FisherWithScore> fishers,
        Dictionary<FisherId, IEnumerable<FishCaught>> fisherCaughtFish,
        int startFromFishCount = 0)
    {
        int maxCaughtFishCount = fishers.Max(f => fisherCaughtFish[f.FisherId].Count());
        int tiesToBreak = fishers.Count;

        for (int i = startFromFishCount; i < maxCaughtFishCount; i++)
        {
            if (tiesToBreak == 1 || tiesToBreak == 0)
            {
                // If tiesToBreak == 1 the reminder fisher will have -1 as tie discriminator, 
                // making it the last by descending order.
                break;
            }

            // Order them by descending score in this index, if two or more fishers (that were not sorted before -> TieDiscriminator == -1)
            // have the same score in this index, then BreakTieByFirstLargerPiece will be called again with the fishers that have the same
            // score in this index.
            var fishersWithSameScore = fishers.Where(f => f.TieDiscriminator == -1)
                                                .GroupBy(p => fisherCaughtFish[p.FisherId].ElementAtOrDefault(i)?.Score ?? 0)
                                                .Where(group => group.Count() > 1)
                                                .SelectMany(group => group)
                                                .ToList();

            var fishersWithDifferentScore = fishers.Where(f => f.TieDiscriminator == -1)
                                                    .Except(fishersWithSameScore)
                                                    .OrderByDescending(f => fisherCaughtFish[f.FisherId].ElementAtOrDefault(i)?.Score ?? 0)
                                                    .ToList();

            foreach (var fisher in fishersWithDifferentScore)
            {
                var score = fisherCaughtFish[fisher.FisherId].ElementAtOrDefault(i)?.Score ?? 0;

                fisher.TieBreakingReason = TieBreakingReasonGenerator.ByNthLargerPiece(i + 1, score);

                fisher.TieDiscriminator = tiesToBreak--;
            }

            if (fishersWithSameScore.Count > 0)
            {
                BreakTieByLargestNthPiece(fishersWithSameScore, fisherCaughtFish, i + 1);
                break;
            }
        }
    }

    private async Task UpdateTournamentLeaderBoard(TournamentId tournamentId, CategoryId categoryId, CategoryId generalCategoryId, CancellationToken cancellationToken)
    {
        await UpdateTournamentLeaderBoard(tournamentId, categoryId, cancellationToken);
        await UpdateTournamentLeaderBoard(tournamentId, generalCategoryId, cancellationToken);
    }

    private async Task UpdateTournamentLeaderBoard(TournamentId tournamentId, CategoryId categoryId, CancellationToken cancellationToken)
    {
        using var _ = ApplicationMetrics.LeaderboardMetrics.LeaderboardUpdate.Time(new Tag("function", "tournament-update"));
        using var ___ = _instrumentation.ActivitySource.StartActivity("UpdateTournamentLeaderBoard");

        _logger.LogInformation("Updating leaderboard TournamentId: {TournamentId} - CategoryId: {CategoryId}", tournamentId, categoryId);

        var tournamentCompetitionsId = await _context.Competitions
            .Where(c => c.TournamentId == tournamentId)
            .OrderBy(c => c.StartDateTime) // asc
            .Select(c => c.Id)
            .ToListAsync(cancellationToken);

        var leaderBoardRepository = _readModelsUnitOfWork.LeaderBoardRepository;

        var newPositions = leaderBoardRepository.CalculateTournamentCategoryLeaderBoard(tournamentId,
                                                                                        categoryId,
                                                                                        tournamentCompetitionsId);

        var previousPositions = leaderBoardRepository.GetTournamentCategoryLeaderBoard(tournamentId,
                                                                                      categoryId);

        var fisherCompetitionPositions = leaderBoardRepository.GetFisherCompetitionPositions(tournamentCompetitionsId, categoryId);

        // sort competitions
        foreach (var fisherPositions in fisherCompetitionPositions.Values)
        {
            fisherPositions.Sort((a, b) => tournamentCompetitionsId.IndexOf(a.Item1)
                                                                   .CompareTo(tournamentCompetitionsId.IndexOf(b.Item1)));
        }

        // sort newPositions ascending by sum of positions and apply tie-breaking rules
        newPositions = newPositions.OrderBy(p => p.PositionsSum)
                            // TODO: Here it goes -2 positions?
                            .ThenByDescending(p => p.TotalScore) // higher total score is better
                            .ThenBy(p => p.FisherId.Value) // else decide by fisherId 

                            .ToList();

        _logger.LogInformation($"Will update {newPositions.Count} positions");

        // update the positions, starting from 1
        for (int i = 0; i < newPositions.Count; i++)
        {
            var newPosition = newPositions[i];
            var existing = previousPositions.FirstOrDefault(p => p.FisherId == newPosition.FisherId);
            var positions = fisherCompetitionPositions[newPosition.FisherId].Select(p => p.Item2).ToList();

            if (existing is not null)
            {
                existing.Position = i + 1;
                existing.TotalScore = newPosition.TotalScore;
                existing.Positions = positions;

                leaderBoardRepository.UpdateTournamentLeaderBoardItem(existing);
            } else
            {
                leaderBoardRepository.AddTournamentLeaderBoardItem(
                    new LeaderboardTournamentCategoryItem
                    {
                        TournamentId = tournamentId,
                        CategoryId = categoryId,
                        FisherId = newPosition.FisherId,
                        Position = i + 1,
                        TotalScore = newPosition.TotalScore,
                        Positions = positions
                    });
            }

            _logger.LogDebug($"FisherId: {newPosition.FisherId} - Position: {i + 1}\n\t TotalScore: {newPosition.TotalScore} - PositionsSum: {newPosition.PositionsSum} \n\t PreviousPosition: {existing?.Position} - PreviousTotalScore: {existing?.TotalScore}");
        }

        _readModelsUnitOfWork.Commit();
    }

    private static void UpdateFisherPositionOnCompetition(CompetitionId competitionId,
                                                   CategoryId categoryId,
                                                   ILeaderBoardRepository leaderBoardRepository,
                                                   IEnumerable<LeaderboardCompetitionCategoryItem> previousCategoryLeaderBoard,
                                                   int newPosition,
                                                   int newScore,
                                                   FisherId fisherId,
                                                   string? tieBreakingReason)
    {
        var previousLeaderBoardItem = previousCategoryLeaderBoard
                                                .FirstOrDefault(f => f.FisherId == fisherId);
        var previousPosition = previousLeaderBoardItem?.Position;
        var previousScore = previousLeaderBoardItem?.Score;
        var prevTieBreakingReason = previousLeaderBoardItem?.TieBreakingReason;

        if (newPosition != previousPosition || newScore != previousScore || prevTieBreakingReason != tieBreakingReason)
        {
            if (previousLeaderBoardItem is not null)
            {
                previousLeaderBoardItem.Position = newPosition;
                previousLeaderBoardItem.Score = newScore;
                previousLeaderBoardItem.TieBreakingReason = tieBreakingReason;

                leaderBoardRepository.UpdateCompetitionLeaderBoardItem(previousLeaderBoardItem);
            } else
            {
                leaderBoardRepository.AddCompetitionLeaderBoardItem(
                    new LeaderboardCompetitionCategoryItem
                    {
                        CompetitionId = competitionId,
                        CategoryId = categoryId,
                        FisherId = fisherId,
                        Position = newPosition,
                        Score = newScore,
                        TieBreakingReason = tieBreakingReason
                    });
            }

            // possible integration event: FisherCompetitionPositionChanged
        }
    }
}