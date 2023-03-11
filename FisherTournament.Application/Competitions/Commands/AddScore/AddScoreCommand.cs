using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.TournamentAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Competitions.Commands.AddScore;

public record struct AddScoreCommand(
    Guid FisherId,
    Guid CompetitionId,
    int Score) : IRequest;

public class AddScoreCommandHandler : IRequestHandler<AddScoreCommand>
{
    private readonly ITournamentFisherDbContext _context;

    public AddScoreCommandHandler(ITournamentFisherDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AddScoreCommand request, CancellationToken cancellationToken)
    {
        // Validate that the fisher and competition exist.
        Fisher? fisher = await _context.Fishers
            .FirstOrDefaultAsync(f => f.Id == request.FisherId, cancellationToken);

        if (fisher is null)
        {
            throw new ApplicationException("Fisher not found");
        }

        Competition? competition = await _context.Competitions
            .FirstOrDefaultAsync(c => c.Id == request.CompetitionId, cancellationToken);

        if (competition is null)
        {
            throw new ApplicationException("Competition not found");
        }

        // Verify the fisher is enrolled in the competition tournament.
        Tournament tournament = (await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Id == competition.TournamentId, cancellationToken))!;

        if (!tournament.IsFisherEnrolled(request.FisherId))
        {
            throw new ApplicationException("Fisher is not enrolled in the tournament");
        }

        competition.AddScore(fisher.Id, request.Score);

        await _context.SaveChangesAsync(cancellationToken);
    }
}