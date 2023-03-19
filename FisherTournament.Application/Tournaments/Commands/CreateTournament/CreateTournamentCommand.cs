using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.TournamentAggregate;
using MediatR;

namespace FisherTournament.Application.Tournaments.Commands.CreateTournament;

public record struct CreateTournamentCommand(
    string Name,
    DateTime StartDate,
    DateTime EndDate) : IRequest<ErrorOr<CreateTournamentCommandResponse>>;

public sealed class CreateTournamentCommandHandler
    : IRequestHandler<CreateTournamentCommand, ErrorOr<CreateTournamentCommandResponse>>
{
    private readonly ITournamentFisherDbContext _context;

    public CreateTournamentCommandHandler(ITournamentFisherDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<CreateTournamentCommandResponse>> Handle(
        CreateTournamentCommand request,
        CancellationToken cancellationToken)
    {
        Tournament tournament = Tournament.Create(
            request.Name,
            request.StartDate,
            request.EndDate);

        _context.Tournaments.Add(tournament);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreateTournamentCommandResponse(tournament.Id, tournament.Name, tournament.StartDate, tournament.EndDate);
    }
}