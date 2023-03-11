using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.UserAggregate;
using MediatR;

namespace FisherTournament.Application.Fishers.Commands.CreateFisher;

public record struct CreateFisherCommand(string FirstName, string LastName)
    : IRequest<CreateFisherCommandResponse>;

public class CreateFisherCommandHandler
 : IRequestHandler<CreateFisherCommand, CreateFisherCommandResponse>
{
    private readonly ITournamentFisherDbContext _context;

    public CreateFisherCommandHandler(ITournamentFisherDbContext context)
    {
        _context = context;
    }

    public async Task<CreateFisherCommandResponse> Handle(CreateFisherCommand request, CancellationToken cancellationToken)
    {
        User user = User.Create(request.FirstName, request.LastName);
        await _context.Users.AddAsync(user);

        Fisher fisher = Fisher.Create(user.Id);

        await _context.Fishers.AddAsync(fisher);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateFisherCommandResponse(fisher.Id, user.FirstName, user.LastName);
    }
}
