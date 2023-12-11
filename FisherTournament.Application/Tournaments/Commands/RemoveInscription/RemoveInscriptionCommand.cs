using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Tournaments.Commands.RemoveInscription
{
    public record struct RemoveInscriptionCommand(string TournamentId, string FisherId)
        : IRequest<ErrorOr<Deleted>>;

    public class RemoveInscriptionCommandHandler
        : IRequestHandler<RemoveInscriptionCommand, ErrorOr<Deleted>>
    {
        private readonly ITournamentFisherDbContext _context;
        private readonly IDateTimeProvider _dateTimeProvider;

        public RemoveInscriptionCommandHandler(ITournamentFisherDbContext context, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<ErrorOr<Deleted>> Handle(RemoveInscriptionCommand request, CancellationToken cancellationToken)
        {
            var tournamentId = TournamentId.Create(request.TournamentId);

            if (tournamentId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(request.TournamentId));
            }

            var fisherId = FisherId.Create(request.FisherId);

            if (fisherId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(request.FisherId));
            }

            var tournament = await _context.Tournaments.Where(t => t.Id == tournamentId.Value).FirstOrDefaultAsync(cancellationToken);

            if (tournament is null)
            {
                return Errors.Tournaments.NotFound;
            }

            var fisherExists = await _context.Fishers.Where(f => f.Id == fisherId.Value).AnyAsync(cancellationToken);

            if (!fisherExists)
            {
                return Errors.Fishers.NotFound;
            }

            var removed = tournament.RemoveInscription(fisherId.Value, _dateTimeProvider);

            if (removed.IsError) return removed.Errors;

            var competitions = await _context.Competitions.Where(c => c.TournamentId == tournamentId.Value).ToListAsync(cancellationToken);

            competitions.ForEach(c => c.RemoveParticipation(fisherId.Value));

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}
