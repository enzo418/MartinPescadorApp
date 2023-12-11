using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Competitions.Commands.AddParticipation;

public record struct AddParticipationCommand(
    string FisherId,
    string CompetitionId) : IRequest<ErrorOr<Success>>;

public class AddParticipationCommandHandler : IRequestHandler<AddParticipationCommand, ErrorOr<Success>>
{
    private readonly ITournamentFisherDbContext _context;

    public AddParticipationCommandHandler(ITournamentFisherDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Success>> Handle(AddParticipationCommand request, CancellationToken cancellationToken)
    {
        var competitionId = CompetitionId.Create(request.CompetitionId);

        if (competitionId.IsError)
        {
            return Errors.Id.NotValidWithProperty(nameof(request.CompetitionId));
        }

        var competition = await _context.Competitions.FirstOrDefaultAsync(c => c.Id == competitionId.Value, cancellationToken);

        if (competition is null)
        {
            return Errors.Competitions.NotFound;
        }

        var fisherId = FisherId.Create(request.FisherId);

        if (fisherId.IsError)
        {
            return Errors.Id.NotValidWithProperty(nameof(request.FisherId));
        }

        var tournament = await _context.Tournaments.FirstOrDefaultAsync(t => t.Id == competition.TournamentId, cancellationToken);

        if (tournament is null)
        {
            return Errors.Tournaments.NotFound;
        }

        if (tournament.IsFisherEnrolled(fisherId.Value) is false)
        {
            return Errors.Tournaments.NotEnrolled;
        }

        var fisher = _context.Fishers.FirstOrDefaultAsync(f => f.Id == fisherId.Value, cancellationToken);

        if (fisher is null)
        {
            return Errors.Fishers.NotFound;
        }

        competition.AddParticipation(fisherId.Value);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }
}