using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.UserAggregate;
using MediatR;

namespace FisherTournament.Application.Fishers.Commands.CreateFisher;

public record struct CreateFisherCommand(string FirstName, string LastName)
    : IRequest<ErrorOr<CreateFisherCommandResponse>>;

public class CreateFisherCommandHandler
 : IRequestHandler<CreateFisherCommand, ErrorOr<CreateFisherCommandResponse>>
{
    private readonly ITournamentFisherDbContext _context;

    public CreateFisherCommandHandler(ITournamentFisherDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<CreateFisherCommandResponse>> Handle(CreateFisherCommand request,
                                                                   CancellationToken cancellationToken)
    {
        Fisher fisher = Fisher.Create(request.FirstName, request.LastName);
        await _context.Fishers.AddAsync(fisher);

        User user = User.Create(request.FirstName, request.LastName, fisher.Id);
        await _context.Users.AddAsync(user);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateFisherCommandResponse(fisher.Id, user.FirstName, user.LastName);
    }
}
