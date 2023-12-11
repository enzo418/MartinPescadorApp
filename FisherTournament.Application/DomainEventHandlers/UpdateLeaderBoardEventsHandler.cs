using FisherTournament.Application.Common.Persistence;
using FisherTournament.Application.LeaderBoard;
using FisherTournament.Domain.CompetitionAggregate.DomainEvents;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.DomainEvents;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FisherTournament.Application.DomainEventHandlers;

/// <summary>
/// This class handles all the domains events that will trigger a leaderboard update.
/// It's goal is to ensure to intialize and keep the leaderboards up to date.
/// </summary>
public class UpdateLeaderBoardEventsHandler
 : INotificationHandler<ScoreAddedDomainEvent>,
    INotificationHandler<ParticipationAddedDomainEvent>,
    INotificationHandler<InscriptionAddedDomainEvent>,
    INotificationHandler<CompetitionAddedDomainEvent>
{
    private readonly ILeaderBoardUpdateScheduler _leaderBoardUpdateScheduler;
    private readonly ITournamentFisherDbContext _context;
    private readonly ILogger<UpdateLeaderBoardEventsHandler> _logger;

    public UpdateLeaderBoardEventsHandler(
        ILeaderBoardUpdateScheduler leaderBoardUpdateScheduler,
        ITournamentFisherDbContext context,
        ILogger<UpdateLeaderBoardEventsHandler> logger)
    {
        _leaderBoardUpdateScheduler = leaderBoardUpdateScheduler;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// This event and handler will make sure the following leaderboards are up to date:
    ///     - The competition leaderboard
    ///         - Specific category, then
    ///         - General category
    ///     - The tournament leaderboard
    ///         - Specific category, then
    ///         - General category
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Handle(ScoreAddedDomainEvent notification,
                             CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ScoreAddedDomainEvent");

        (TournamentId? tournamentId, CategoryId? categoryId) = await GetTournamentAndCategoryIds(notification.CompetitionId,
                                                                                                 notification.FisherId,
                                                                                                 cancellationToken);

        if (tournamentId == null || categoryId == null)
        {
            return;
        }

        _leaderBoardUpdateScheduler.ScheduleLeaderBoardUpdate(tournamentId, notification.CompetitionId, categoryId);
    }

    /// <summary>
    /// This event will make sure the leaderboard differientiates between the fishers that have participated in the 
    /// competition and the ones that have not.
    /// It will update the specific category leaderboard and the general category leaderboard.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Handle(ParticipationAddedDomainEvent notification,
                             CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ParticipationAddedDomainEvent");

        (TournamentId? tournamentId, CategoryId? categoryId) = await GetTournamentAndCategoryIds(notification.CompetitionId,
                                                                                                 notification.FisherId,
                                                                                                 cancellationToken);

        if (tournamentId == null || categoryId == null)
        {
            return;
        }

        _leaderBoardUpdateScheduler.ScheduleLeaderBoardUpdate(tournamentId, notification.CompetitionId, categoryId);
    }

    /// <summary>
    /// This event and handler will make sure the leaderboard has all the fishers that have enrolled in the tournament.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task Handle(InscriptionAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling InscriptionAddedDomainEvent");

        _leaderBoardUpdateScheduler.ScheduleLeaderBoardUpdate(notification.TournamentId, notification.CategoryId);

        await Task.CompletedTask;
    }

    /// <summary>
    /// This event and handler will set the initial leaderboard for the competition.
    /// This means that every fisher will have the same position.
    /// </summary>
    /// <param name="notification"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task Handle(CompetitionAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling CompetitionAddedDomainEvent");

        List<CategoryId> categories = await GetTournamentCategories(notification.TournamentId, cancellationToken);

        foreach (var category in categories)
        {
            _leaderBoardUpdateScheduler.ScheduleLeaderBoardUpdate(notification.TournamentId, notification.CompetitionId, category);
        }
    }

    private async Task<(TournamentId?, CategoryId?)> GetTournamentAndCategoryIds(CompetitionId competitionId,
                                                                                 FisherId fisherId,
                                                                                 CancellationToken cancellationToken)
    {
        var query =
            from c in _context.Competitions
            join t in _context.Tournaments on c.TournamentId equals t.Id
            from inscription in t.Inscriptions
            where c.Id == competitionId
            where inscription.FisherId == fisherId
            select new { inscription.CategoryId, t.Id };

        var res = (await query.ToListAsync(cancellationToken)).FirstOrDefault();

        return (res?.Id, res?.CategoryId);
    }

    private Task<List<CategoryId>> GetTournamentCategories(TournamentId tournamentId, CancellationToken cancellationToken) =>
         _context.Tournaments
            .Where(t => t.Id == tournamentId)
            .SelectMany(t => t.Categories.Select(c => c.Id))
            .ToListAsync(cancellationToken);
}