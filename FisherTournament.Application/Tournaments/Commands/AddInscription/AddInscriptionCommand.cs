using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.TournamentAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Tournaments.Commands.AddInscription;

public record struct AddInscriptionCommand(
    Guid TournamentId,
    Guid FisherId) : IRequest;

public class AddInscriptionCommandHandler : IRequestHandler<AddInscriptionCommand>
{
    private readonly ITournamentFisherDbContext _context;

    public AddInscriptionCommandHandler(ITournamentFisherDbContext context)
    {
        _context = context;
    }

    public async Task Handle(AddInscriptionCommand request, CancellationToken cancellationToken)
    {
        Tournament? tournament = await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);

        if (tournament is null)
        {
            throw new ApplicationException("Tournament not found");
        }

        Fisher? fisher = await _context.Fishers
            .FirstOrDefaultAsync(f => f.Id == request.FisherId, cancellationToken);

        if (fisher is null)
        {
            throw new ApplicationException("Fisher not found");
        }

        tournament.AddInscription(fisher.Id);

        await _context.SaveChangesAsync(cancellationToken);
    }
}