using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Tournaments.Commands.AddCompetitions;

public record struct AddCompetitionsCommand(
    TournamentId TournamentId,
    List<AddCompetitionCommand> Competitions) : IRequest<List<Competition>>;

public record struct AddCompetitionCommand(
    DateTime StartDateTime,
    string City,
    string State,
    string Country,
    string Place
);

public class AddCompetitionsCommandHandler
    : IRequestHandler<AddCompetitionsCommand, List<Competition>>
{
    private readonly ITournamentFisherDbContext _context;

    public AddCompetitionsCommandHandler(ITournamentFisherDbContext context)
    {
        _context = context;
    }

    public async Task<List<Competition>> Handle(AddCompetitionsCommand request, CancellationToken cancellationToken)
    {
        Tournament? tournament = await _context.Tournaments
            .FirstOrDefaultAsync(t => t.Id == request.TournamentId, cancellationToken);

        if (tournament is null)
        {
            throw new ApplicationException("Tournament not found");
        }

        Competition[] competitions = request.Competitions.Select(
            competition => Competition.Create(
                competition.StartDateTime,
                request.TournamentId,
                Location.Create(
                    competition.City,
                    competition.State,
                    competition.Country,
                    competition.Place))).ToArray();

        _context.Competitions.AddRange(competitions);

        foreach (Competition competition in competitions)
        {
            tournament.AddCompetition(competition.Id);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new List<Competition>(competitions.ToList());
    }
}