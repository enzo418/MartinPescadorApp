using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Competitions.Commands.BulkEditScores;

public record struct BulkEditScoresItem(
    string FisherId,
    IEnumerable<int> Scores);

public record struct BulkEditScoresCommand(
    string TournamentId,
    string CompetitionId,
    List<BulkEditScoresItem> Items) : IRequest<ErrorOr<Updated>>;

public class BulkEditScoresCommandHandler : IRequestHandler<BulkEditScoresCommand, ErrorOr<Updated>>
{
    private readonly ITournamentFisherDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public BulkEditScoresCommandHandler(ITournamentFisherDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ErrorOr<Updated>> Handle(BulkEditScoresCommand request, CancellationToken cancellationToken)
    {
        ErrorOr<CompetitionId> competitionId = CompetitionId.Create(request.CompetitionId);

        if (competitionId.IsError)
        {
            return Errors.Id.NotValidWithProperty(nameof(request.CompetitionId));
        }

        // Validate that the fisher and competition exist.
        Competition? competition = await _context.Competitions
            .Include(c => c.Participations)
            .FirstOrDefaultAsync(c => c.Id == competitionId.Value, cancellationToken);

        if (competition is null)
        {
            return Errors.Competitions.NotFound;
        }

        // Verify the fisher is enrolled in the competition tournament.
        Tournament tournament = (await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Id == competition.TournamentId, cancellationToken))!;

        if (tournament is null)
        {
            return Errors.Tournaments.NotFound;
        }

        foreach (var item in request.Items)
        {
            var fisherId = FisherId.Create(item.FisherId);

            if (fisherId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(item.FisherId));
            }

            if (!tournament.IsFisherEnrolled(fisherId.Value))
            {
                return Errors.Tournaments.NotEnrolled;
            }

            competition.RemoveParticipation(fisherId.Value);

            if (item.Scores.Any())
            {
                competition.AddScores(fisherId.Value, item.Scores, _dateTimeProvider);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new Updated();
    }
}
