using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Application.Common.Resources;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Tournaments.Commands.AddCompetitions;

public record struct AddCompetitionsCommand(
	string TournamentId,
	List<AddCompetitionCommand> Competitions) : IRequest<ErrorOr<List<AddCompetitionCommandResponse>>>;

public record struct AddCompetitionCommandResponse(
	string Id,
	DateTime StartDateTime,
	CompetitionLocationResource Location);

public record struct AddCompetitionCommand(
	DateTime StartDateTime,
	CompetitionLocationResource Location);

public class AddCompetitionsCommandHandler
	: IRequestHandler<AddCompetitionsCommand, ErrorOr<List<AddCompetitionCommandResponse>>>
{
	private readonly ITournamentFisherDbContext _context;

	public AddCompetitionsCommandHandler(ITournamentFisherDbContext context)
	{
		_context = context;
	}

	public async Task<ErrorOr<List<AddCompetitionCommandResponse>>> Handle(AddCompetitionsCommand request, CancellationToken cancellationToken)
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
			return Errors.Tournaments.NotFound;
		}

		if (request.Competitions.Any(c => c.StartDateTime < tournament.StartDate))
		{
			return Errors.Competitions.StartDateBeforeTournament;
		}

		int totalCompetitions = tournament.CompetitionsIds.Count;

		Competition[] competitions = request.Competitions.Select(
			competition => Competition.Create(
				competition.StartDateTime,
				tournamentId.Value,
				Location.Create(
					competition.Location.City,
					competition.Location.State,
					competition.Location.Country,
					competition.Location.Place),
				++totalCompetitions
				)).ToArray();

		_context.Competitions.AddRange(competitions);

		foreach (Competition competition in competitions)
		{
			tournament.AddCompetition(competition.Id);
		}

		await _context.SaveChangesAsync(cancellationToken);

		return competitions.Select(c => new AddCompetitionCommandResponse(
			c.Id.ToString(),
			c.StartDateTime,
			new CompetitionLocationResource(c.Location.City,
			c.Location.State,
			c.Location.Country,
			c.Location.Place))).ToList();
	}
}