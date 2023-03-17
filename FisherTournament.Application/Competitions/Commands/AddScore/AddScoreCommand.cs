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
    FisherId FisherId,
    CompetitionId CompetitionId,
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
        // Validate that the fisher and competition exist.
        Fisher? fisher = await _context.Fishers
            .FirstOrDefaultAsync(f => f.Id == request.FisherId, cancellationToken);

        if (fisher is null)
        {
            return Errors.Fisher.NotFound;
        }

        Competition? competition = await _context.Competitions
            .FirstOrDefaultAsync(c => c.Id == request.CompetitionId, cancellationToken);

        if (competition is null)
        {
            return Errors.Competition.NotFound;
        }

        // Verify the fisher is enrolled in the competition tournament.
        Tournament tournament = (await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Id == competition.TournamentId, cancellationToken))!;

        if (!tournament.IsFisherEnrolled(request.FisherId))
        {
            return Errors.Tournament.NotEnrolled;
        }

        competition.AddScore(fisher.Id, request.Score, _dateTimeProvider);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}