using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.CompetitionAggregate.DomainEvents;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.DomainEvents;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.ReadModels.Models;
using FisherTournament.ReadModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.DomainEventHandlers;

public class FisherWithScore
{
    public FisherId FisherId { get; init; } = null!;
    public int Score { get; init; }
}

public class UpdateLeaderBoardEventsHandler
 : INotificationHandler<ScoreAddedDomainEvent>,
    INotificationHandler<ParticipationAddedDomainEvent>,
    INotificationHandler<InscriptionAddedDomainEvent>
{
    private readonly ITournamentFisherDbContext _context;
    private readonly IReadModelsUnitOfWork _readModelsUnitOfWork;

    public UpdateLeaderBoardEventsHandler(ITournamentFisherDbContext context, IReadModelsUnitOfWork readModelsUnitOfWork)
    {
        _context = context;
        _readModelsUnitOfWork = readModelsUnitOfWork;
    }

    public async Task Handle(ScoreAddedDomainEvent notification,
                             CancellationToken cancellationToken)
    {
        await UpdateLeaderBoard(notification.CompetitionId,
                                notification.FisherId,
                                cancellationToken);
    }

    public async Task Handle(ParticipationAddedDomainEvent notification,
                             CancellationToken cancellationToken)
    {
        await UpdateLeaderBoard(notification.CompetitionId,
                                notification.FisherId,
                                cancellationToken);
    }
    
    public async Task Handle(InscriptionAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        // this will be an expensive operation, might be opt-in
        await UpdateLeaderBoard(notification.TournamentId, notification.CategoryId, cancellationToken);
    }
    
    private async Task UpdateLeaderBoard(TournamentId tournamentId,
                                            CategoryId categoryId,
                                            CancellationToken cancellationToken)
    {
        var competitionsIds = await _context.Competitions
            .Where(x => x.TournamentId == tournamentId)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        foreach (var competitionId in competitionsIds)   
        {
            await UpdateCompetitionLeaderBoard(competitionId, categoryId);
        }
        
        await UpdateTournamentLeaderBoard(tournamentId, categoryId);
    }

    private async Task UpdateLeaderBoard(CompetitionId competitionId,
                                        FisherId fisherId,
                                        CancellationToken cancellationToken)
    {
        var tournamentId = await _context.Competitions
            .Where(x => x.Id == competitionId)
            .Select(x => x.TournamentId)
            .FirstOrDefaultAsync(cancellationToken);

        if (tournamentId == null)
        {
            return;
        }

        var categoryId = await _context.Tournaments
            .Where(t => t.Id == tournamentId)
            .SelectMany(t => t.Inscriptions)
            .Where(i => i.FisherId == fisherId)
            .Select(i => i.CategoryId)
            .FirstOrDefaultAsync(cancellationToken);

        if (categoryId == null)
        {
            return;
        }
        
        await UpdateCompetitionLeaderBoard(competitionId, categoryId);
        await UpdateTournamentLeaderBoard(tournamentId, categoryId);
    }

    private async Task UpdateCompetitionLeaderBoard(CompetitionId competitionId, CategoryId categoryId)
    {
        // 2. Get each fisher and score from the this category
        // TODO: This query screams for a refactor, don't worry
        var fishersFromCategoryWithScore = await (
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
                        Score = p == null ? -1 : p.TotalScore
                    }
                ).ToListAsync();

        // 3. Sort the fishers by descending score
        fishersFromCategoryWithScore.Sort((x, y) => x.Score.CompareTo(y.Score) * -1);

        // 4. Get the fishers positions respecting the system rules
        var leaderBoardRepository = _readModelsUnitOfWork.LeaderBoardRepository;

        var previousCategoryLeaderBoard = leaderBoardRepository
                                                .GetCompetitionCategoryLeaderBoard(competitionId,
                                                                                   categoryId);

        int currentPosition = 0;
        int previousScore = int.MaxValue;

        foreach (var fisher in fishersFromCategoryWithScore)
        {
            if (fisher.Score != previousScore)
            {
                currentPosition++;
            }

            UpdateFisherPositionOnCompetition(competitionId,
                                              categoryId,
                                              leaderBoardRepository,
                                              previousCategoryLeaderBoard,
                                              newPosition: currentPosition,
                                              newScore: fisher.Score,
                                              fisher.FisherId);

            previousScore = fisher.Score;
        }

        _readModelsUnitOfWork.Commit();
    }


    private async Task UpdateTournamentLeaderBoard(TournamentId tournamentId, CategoryId categoryId)
    {
        var tournamentCompetitionsId = await _context.Competitions
            .Where(c => c.TournamentId == tournamentId)
            .Select(c => c.Id)
            .ToListAsync();

        var leaderBoardRepository = _readModelsUnitOfWork.LeaderBoardRepository;

        var newPositions = leaderBoardRepository.CalculateTournamentCategoryLeaderBoard(tournamentId,
                                                                                        categoryId,
                                                                                        tournamentCompetitionsId);

        var previousPositions = leaderBoardRepository.GetTournamentCategoryLeaderBoard(tournamentId,
                                                                                      categoryId);

        // sort newPositions ascending by sum of positions and apply disambiguation rules
        newPositions = newPositions.OrderBy(p => p.PositionsSum)
                            // Disambiguation rules
                            .ThenBy(p => p.AveragePosition) // lower average position is better
                            .ThenByDescending(p => p.TotalScore) // higher total score is better
                            .ThenBy(p => p.FisherId) // else decide by fisherId 

                            .ToList();

        // update the positions, starting from 1
        for (int i = 0; i < newPositions.Count; i++)
        {
            var newPosition = newPositions[i];
            var existing = previousPositions.FirstOrDefault(p => p.FisherId == newPosition.FisherId);

            if (existing is not null)
            {
                existing.Position = i + 1;
                existing.TotalScore = newPosition.TotalScore;

                leaderBoardRepository.UpdateTournamentLeaderBoardItem(existing);
            }
            else
            {
                leaderBoardRepository.AddTournamentLeaderBoardItem(
                    new LeaderboardTournamentCategoryItem
                    {
                        TournamentId = tournamentId,
                        CategoryId = categoryId,
                        FisherId = newPosition.FisherId,
                        Position = i + 1,
                        TotalScore = newPosition.TotalScore
                    });
            }

            // TODO: Add proper logging
            Console.WriteLine($"FisherId: {newPosition.FisherId} - Position: {i + 1}\n\t TotalScore: {newPosition.TotalScore} - PositionsSum: {newPosition.PositionsSum} - AveragePosition: {newPosition.AveragePosition}\n\t PreviousPosition: {existing?.Position} - PreviousTotalScore: {existing?.TotalScore}");
        }

        _readModelsUnitOfWork.Commit();
    }
    
    private static void UpdateFisherPositionOnCompetition(CompetitionId competitionId,
                                                   CategoryId categoryId,
                                                   ILeaderBoardRepository leaderBoardRepository,
                                                   IEnumerable<LeaderboardCompetitionCategoryItem> previousCategoryLeaderBoard,
                                                   int newPosition,
                                                   int newScore,
                                                   FisherId fisherId)
    {
        var previousLeaderBoardItem = previousCategoryLeaderBoard
                                                .FirstOrDefault(f => f.FisherId == fisherId);
        var previousPosition = previousLeaderBoardItem?.Position;
        var previousScore = previousLeaderBoardItem?.Score;

        if (newPosition != previousPosition || newScore != previousScore)
        {
            if (previousLeaderBoardItem is not null)
            {
                previousLeaderBoardItem.Position = newPosition;
                previousLeaderBoardItem.Score = newScore;

                leaderBoardRepository.UpdateCompetitionLeaderBoardItem(previousLeaderBoardItem);
            }
            else
            {
                leaderBoardRepository.AddCompetitionLeaderBoardItem(
                    new LeaderboardCompetitionCategoryItem
                    {
                        CompetitionId = competitionId,
                        CategoryId = categoryId,
                        FisherId = fisherId,
                        Position = newPosition,
                        Score = newScore,
                    });
            }

            // possible integration event: LeaderBoardUpdated, FisherCompetitionPositionChanged
        }

        // TODO: log
        Console.WriteLine($"FisherId: {fisherId}, Score: {newScore}, Position: {newPosition} - PreviousPosition: {previousPosition}, PreviousScore: {previousScore}");
    }
}