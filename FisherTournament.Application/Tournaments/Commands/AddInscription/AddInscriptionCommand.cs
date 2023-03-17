using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FisherTournament.Domain.Common.Errors;

namespace FisherTournament.Application.Tournaments.Commands.AddInscription;

public record struct AddInscriptionCommand(
    TournamentId TournamentId,
    FisherId FisherId) : IRequest<ErrorOr<Created>>;

public class AddInscriptionCommandHandler : IRequestHandler<AddInscriptionCommand, ErrorOr<Created>>
{
    private readonly ITournamentFisherDbContext _context;

    public AddInscriptionCommandHandler(ITournamentFisherDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Created>> Handle(AddInscriptionCommand request, CancellationToken cancellationToken)
    {
        Tournament? tournament = await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);

        if (tournament is null)
        {
            return Errors.Tournament.NotFound;
        }

        Fisher? fisher = await _context.Fishers
            .FirstOrDefaultAsync(f => f.Id == request.FisherId, cancellationToken);

        if (fisher is null)
        {
            return Errors.Fisher.NotFound;
        }

        tournament.AddInscription(fisher.Id);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Created;
    }
}