using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.UserAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Fishers.Commands.CreateFisher;

public record struct CreateFisherCommand(string FirstName, string LastName, string DNI)
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

        bool dniExists = await _context.Users.AnyAsync(u => u.DNI == request.DNI, cancellationToken);

        if (dniExists)
        {
            return Errors.Users.DNIAlreadyExists;
        }

        await _context.Fishers.AddAsync(fisher, cancellationToken);

        User user = User.Create(request.FirstName, request.LastName, request.DNI, fisher.Id);
        await _context.Users.AddAsync(user, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateFisherCommandResponse(fisher.Id, user.FirstName, user.LastName, user.DNI);
    }
}
