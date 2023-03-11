using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.TournamentAggregate;
using MediatR;

namespace FisherTournament.Application.Tournaments.Commands.CreateTournament;

public record struct CreateTournamentCommand(
    string Name,
    DateTime startDate,
    DateTime endDate) : IRequest<CreateTournamentCommandResponse>;

public sealed class CreateTournamentCommandHandler
    : IRequestHandler<CreateTournamentCommand, CreateTournamentCommandResponse>
{
    private readonly ITournamentFisherDbContext _context;

    public CreateTournamentCommandHandler(ITournamentFisherDbContext context)
    {
        _context = context;
    }

    public async Task<CreateTournamentCommandResponse> Handle(
        CreateTournamentCommand request,
        CancellationToken cancellationToken)
    {
        Tournament tournament = Tournament.Create(
            request.Name,
            request.startDate,
            request.endDate);

        _context.Tournaments.Add(tournament);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateTournamentCommandResponse(tournament.Id, tournament.Name, tournament.StartDate, tournament.EndDate);
    }
}