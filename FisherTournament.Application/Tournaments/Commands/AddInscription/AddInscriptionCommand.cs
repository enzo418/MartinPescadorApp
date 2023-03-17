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
    string TournamentId,
    string FisherId) : IRequest<ErrorOr<Created>>;

public class AddInscriptionCommandHandler : IRequestHandler<AddInscriptionCommand, ErrorOr<Created>>
{
    private readonly ITournamentFisherDbContext _context;

    public AddInscriptionCommandHandler(ITournamentFisherDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<Created>> Handle(AddInscriptionCommand request, CancellationToken cancellationToken)
    {
        ErrorOr<TournamentId> tournamentId = TournamentId.Create(request.TournamentId);

        if (tournamentId.IsError)
        {
            return Errors.Id.NotValidWithProperty(nameof(request.TournamentId));
        }

        Tournament? tournament = await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Id == tournamentId.Value, cancellationToken);

        if (tournament is null)
        {
            return Errors.Tournament.NotFound;
        }

        ErrorOr<FisherId> fisherId = FisherId.Create(request.FisherId);

        if (fisherId.IsError)
        {
            return Errors.Id.NotValidWithProperty(nameof(request.FisherId));
        }

        Fisher? fisher = await _context.Fishers
            .FirstOrDefaultAsync(f => f.Id == fisherId.Value, cancellationToken);

        if (fisher is null)
        {
            return Errors.Fisher.NotFound;
        }

        tournament.AddInscription(fisher.Id);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Created;
    }
}