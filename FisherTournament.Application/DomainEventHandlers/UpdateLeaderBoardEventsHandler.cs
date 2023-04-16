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
using FisherTournament.Application.Common.Metrics;
using FisherTournament.Application.Common.Instrumentation;
using Microsoft.Extensions.Logging;
using FisherTournament.Application.LeaderBoard;

namespace FisherTournament.Application.DomainEventHandlers;

public class UpdateLeaderBoardEventsHandler
 : INotificationHandler<ScoreAddedDomainEvent>,
    INotificationHandler<ParticipationAddedDomainEvent>,
    INotificationHandler<InscriptionAddedDomainEvent>
{
    private readonly ILeaderBoardUpdateScheduler _leaderBoardUpdateScheduler;
    private readonly ITournamentFisherDbContext _context;

    public UpdateLeaderBoardEventsHandler(
        ILeaderBoardUpdateScheduler leaderBoardUpdateScheduler,
        ITournamentFisherDbContext context)
    {
        _leaderBoardUpdateScheduler = leaderBoardUpdateScheduler;
        _context = context;
    }

    public async Task Handle(ScoreAddedDomainEvent notification,
                             CancellationToken cancellationToken)
    {
        (TournamentId? tournamentId, CategoryId? categoryId) = await GetTournamentAndCategoryIds(notification.CompetitionId, notification.FisherId, cancellationToken);

        if (tournamentId == null || categoryId == null)
        {
            return;
        }

        _leaderBoardUpdateScheduler.ScheduleLeaderBoardUpdate(tournamentId, notification.CompetitionId, categoryId);
    }

    public async Task Handle(ParticipationAddedDomainEvent notification,
                             CancellationToken cancellationToken)
    {
        (TournamentId? tournamentId, CategoryId? categoryId) = await GetTournamentAndCategoryIds(notification.CompetitionId, notification.FisherId, cancellationToken);

        if (tournamentId == null || categoryId == null)
        {
            return;
        }

        _leaderBoardUpdateScheduler.ScheduleLeaderBoardUpdate(tournamentId, notification.CompetitionId, categoryId);
    }

    public async Task Handle(InscriptionAddedDomainEvent notification, CancellationToken cancellationToken)
    {
        // this will be an expensive operation, might be opt-in
        _leaderBoardUpdateScheduler.ScheduleLeaderBoardUpdate(notification.TournamentId, notification.CategoryId);

        await Task.CompletedTask;
    }

    private async Task<(TournamentId?, CategoryId?)> GetTournamentAndCategoryIds(CompetitionId competitionId, FisherId fisherId, CancellationToken cancellationToken)
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

    // private async Task<TournamentId?> GetTournamentId(CompetitionId competitionId, CancellationToken cancellationToken)
    // {
    //     var tournamentId = await _context.Competitions
    //         .Where(x => x.Id == competitionId)
    //         .Select(x => x.TournamentId)
    //         .FirstOrDefaultAsync(cancellationToken);

    //     return tournamentId;
    // }

    // private async Task<CategoryId?> GetCategoryId(TournamentId tournamentId, FisherId fisherId, CancellationToken cancellationToken)
    // {
    //     var categoryId = await _context.Tournaments
    //         .Where(t => t.Id == tournamentId)
    //         .SelectMany(t => t.Inscriptions)
    //         .Where(i => i.FisherId == fisherId)
    //         .Select(i => i.CategoryId)
    //         .FirstOrDefaultAsync(cancellationToken);

    //     return categoryId;
    // }

}