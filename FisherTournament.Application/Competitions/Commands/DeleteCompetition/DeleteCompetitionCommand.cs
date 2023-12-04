using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using MediatR;

namespace FisherTournament.Application.Competitions.Commands.DeleteCompetition
{
    public record struct DeleteCompetitionCommand(string CompetitionId)
        : IRequest<ErrorOr<Deleted>>;

    public class DeleteCompetitionCommandHandler
        : IRequestHandler<DeleteCompetitionCommand, ErrorOr<Deleted>>
    {
        private readonly ITournamentFisherDbContext _context;

        public DeleteCompetitionCommandHandler(ITournamentFisherDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<Deleted>> Handle(DeleteCompetitionCommand request, CancellationToken cancellationToken)
        {
            var competitionId = CompetitionId.Create(request.CompetitionId);

            if (competitionId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(request.CompetitionId));
            }

            var competition = await _context.Competitions
                .FindAsync(new object[] { competitionId.Value }, cancellationToken: cancellationToken);

            if (competition == null)
            {
                return Errors.Competitions.NotFound;
            }

            var tournament = await _context.Tournaments
                .FindAsync(new object[] { competition.TournamentId }, cancellationToken: cancellationToken);

            if (tournament == null)
            {
                return Errors.Tournaments.NotFound;
            }

            tournament.RemoveCompetition(competitionId.Value);

            _context.Competitions.Remove(competition);

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Deleted;
        }
    }
}
