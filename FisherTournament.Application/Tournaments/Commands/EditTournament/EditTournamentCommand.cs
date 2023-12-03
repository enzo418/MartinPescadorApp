using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Tournaments.Commands.EditTournament
{
    public record struct EditTournamentCommand(
        string TournamentId,
        string? Name,
        DateTime? StartDate,
        bool? TournamentFinishedState)
        : IRequest<ErrorOr<EditTournamentCommandResult>>;

    public record struct EditTournamentCommandResult(
        string TournamentId,
        string Name,
        DateTime StartDate,
        DateTime? EndDate);

    public class EditTournamentCommandHandler
        : IRequestHandler<EditTournamentCommand, ErrorOr<EditTournamentCommandResult>>
    {
        private readonly IDateTimeProvider _dateTimeProvider;
        private readonly ITournamentFisherDbContext _context;

        public EditTournamentCommandHandler(ITournamentFisherDbContext context, IDateTimeProvider dateTimeProvider)
        {
            _context = context;
            _dateTimeProvider = dateTimeProvider;
        }

        public async Task<ErrorOr<EditTournamentCommandResult>> Handle(EditTournamentCommand request, CancellationToken cancellationToken)
        {
            var tournamentId = TournamentId.Create(request.TournamentId);

            if (tournamentId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(request.TournamentId));
            }

            var tournament = await _context.Tournaments
                .Where(t => t.Id == tournamentId.Value)
                .FirstOrDefaultAsync(cancellationToken);

            if (tournament == null)
            {
                return Errors.Tournaments.NotFound;
            }

            if (request.StartDate.HasValue && request.StartDate != tournament.StartDate)
            {
                bool competitionHasEarlierStartDate = await _context.Competitions
                    .Where(c => c.TournamentId == tournamentId.Value && c.StartDateTime < request.StartDate)
                    .AnyAsync(cancellationToken);

                if (competitionHasEarlierStartDate)
                {
                    return Errors.Tournaments.CompetitionHasEarlierStartDate;
                }
            }

            if (request.Name is not null && tournament.SetName(request.Name) is var res && res.IsError)
            {
                return res.Errors;
            }

            if (request.StartDate.HasValue && tournament.SetStartDate(request.StartDate.Value) is var dev && dev.IsError)
            {
                return dev.Errors;
            }


            if (request.TournamentFinishedState is not null)
            {
                if (request.TournamentFinishedState.Value && tournament.EndTournament(_dateTimeProvider) is var ended && ended.IsError)
                {
                    return ended.Errors;
                } else if (!request.TournamentFinishedState.Value && tournament.UndoEndTournament() is var restarted && restarted.IsError)
                {
                    return restarted.Errors;
                }
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new EditTournamentCommandResult(
                tournament.Id.ToString(),
                tournament.Name,
                tournament.StartDate,
                tournament.EndDate);
        }
    }
}
