using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Application.Common.Resources;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using MediatR;

namespace FisherTournament.Application.Competitions.Commands.EditCompetition
{
	public record struct EditCompetitionCommand(
		string CompetitionId,
		DateTime? StartDateTime,
		CompetitionLocationResource? Location,
		bool? CompetitionFinishedState)
		: IRequest<ErrorOr<Updated>>;

	public class EditCompetitionCommandHandler
		: IRequestHandler<EditCompetitionCommand, ErrorOr<Updated>>
	{
		private readonly ITournamentFisherDbContext _context;
		private readonly IDateTimeProvider _dateTimeProvider;

		public EditCompetitionCommandHandler(ITournamentFisherDbContext context, IDateTimeProvider dateTimeProvider)
		{
			_context = context;
			_dateTimeProvider = dateTimeProvider;
		}

		public async Task<ErrorOr<Updated>> Handle(EditCompetitionCommand request, CancellationToken cancellationToken)
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


			if (request.StartDateTime is not null)
			{
				if (request.StartDateTime < tournament.StartDate)
				{
					return Errors.Competitions.StartDateBeforeTournament;
				}

				competition.EditStartDate(request.StartDateTime.Value);
			}

			if (request.Location is not null)
			{
				var rqLocation = request.Location.Value;
				Location location = Location.Create(rqLocation.City, rqLocation.State, rqLocation.Country, rqLocation.Place);
				competition.EditLocation(location);
			}

			if (request.CompetitionFinishedState is not null)
			{
				if (request.CompetitionFinishedState.Value) competition.EndCompetition(_dateTimeProvider);
				else competition.UndoEndCompetition();
			}

			await _context.SaveChangesAsync(cancellationToken);

			return Result.Updated;
		}
	}
}
