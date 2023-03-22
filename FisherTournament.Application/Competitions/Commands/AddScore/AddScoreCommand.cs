using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Competitions.Commands.AddScore;

public record struct AddScoreCommand(
    string FisherId,
    string CompetitionId,
    int Score) : IRequest<ErrorOr<Updated>>;

public class AddScoreCommandHandler : IRequestHandler<AddScoreCommand, ErrorOr<Updated>>
{
    private readonly ITournamentFisherDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AddScoreCommandHandler(ITournamentFisherDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ErrorOr<Updated>> Handle(AddScoreCommand request, CancellationToken cancellationToken)
    {
        ErrorOr<FisherId> fisherId = FisherId.Create(request.FisherId);

        if (fisherId.IsError)
        {
            return Errors.Id.NotValidWithProperty(nameof(request.FisherId));
        }

        // Validate that the fisher and competition exist.
        Fisher? fisher = await _context.Fishers
            .FirstOrDefaultAsync(f => f.Id == fisherId.Value, cancellationToken);

        if (fisher is null)
        {
            return Errors.Fishers.NotFound;
        }

        ErrorOr<CompetitionId> competitionId = CompetitionId.Create(request.CompetitionId);

        if (competitionId.IsError)
        {
            return Errors.Id.NotValidWithProperty(nameof(request.CompetitionId));
        }

        Competition? competition = await _context.Competitions
            .FirstOrDefaultAsync(c => c.Id == competitionId.Value, cancellationToken);

        if (competition is null)
        {
            return Errors.Competitions.NotFound;
        }

        // Verify the fisher is enrolled in the competition tournament.
        Tournament tournament = (await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Id == competition.TournamentId, cancellationToken))!;

        if (!tournament.IsFisherEnrolled(fisherId.Value))
        {
            return Errors.Tournaments.NotEnrolled;
        }

        competition.AddScore(fisher.Id, request.Score, _dateTimeProvider);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}