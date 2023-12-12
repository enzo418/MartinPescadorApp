using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Tournaments.Commands.RemoveTournament
{
    public record struct RemoveTournamentCommand(string TournamentId)
        : IRequest<ErrorOr<Deleted>>;

    public class RemoveTournamentCommandHandler
        : IRequestHandler<RemoveTournamentCommand, ErrorOr<Deleted>>
    {
        private readonly ITournamentFisherDbContext _context;

        public RemoveTournamentCommandHandler(ITournamentFisherDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<Deleted>> Handle(RemoveTournamentCommand request, CancellationToken cancellationToken)
        {
            var tournamentId = TournamentId.Create(request.TournamentId);

            if (tournamentId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(request.TournamentId));
            }

            var tournament = await _context.Tournaments.Where(t => t.Id == tournamentId.Value).FirstOrDefaultAsync(cancellationToken);

            if (tournament is null)
            {
                return Errors.Tournaments.NotFound;
            }

            var res = tournament.SetForDeletion();
            if (res.IsError)
            {
                return res.Errors;
            }

            _context.Tournaments.Remove(tournament);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}
