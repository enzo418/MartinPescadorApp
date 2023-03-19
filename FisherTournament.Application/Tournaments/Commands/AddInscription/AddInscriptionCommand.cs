using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.Common.Provider;

namespace FisherTournament.Application.Tournaments.Commands.AddInscription;

public record struct AddInscriptionCommand(
    string TournamentId,
    string FisherId) : IRequest<ErrorOr<Created>>;

public class AddInscriptionCommandHandler : IRequestHandler<AddInscriptionCommand, ErrorOr<Created>>
{
    private readonly ITournamentFisherDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AddInscriptionCommandHandler(ITournamentFisherDbContext context, IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ErrorOr<Created>> Handle(AddInscriptionCommand request, CancellationToken cancellationToken)
    {
        ErrorOr<TournamentId> tournamentId = TournamentId.Create(request.TournamentId);

        if (tournamentId.IsError)
        {
            return Errors.Id.NotValidWithProperty(nameof(request.TournamentId));
        }

        Tournament? tournament = _context.Set<Tournament>()
            .FirstOrDefault(t => t.Id == tournamentId.Value);

        if (tournament is null)
        {
            return Errors.Tournament.NotFound;
        }

        if (tournament.EndDate < _dateTimeProvider.Now)
        {
            return Errors.Tournament.IsOver;
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

        if (tournament.Inscriptions.Any(i => i.FisherId == fisher.Id))
        {
            return Errors.Tournament.InscriptionAlreadyExists;
        }

        tournament.AddInscription(fisher.Id);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Created;
    }
}