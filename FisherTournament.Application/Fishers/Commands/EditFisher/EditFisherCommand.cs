using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.UserAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Fishers.Commands.EditFisher;

public record struct EditFisherCommand(FisherId FisherId, string NewFirstName, string NewLastName, string NewDNI)
    : IRequest<ErrorOr<EditFisherCommandResponse>>;

public class EditFisherCommandHandler
 : IRequestHandler<EditFisherCommand, ErrorOr<EditFisherCommandResponse>>
{
    private readonly ITournamentFisherDbContext _context;

    public EditFisherCommandHandler(ITournamentFisherDbContext context)
    {
        _context = context;
    }

    public async Task<ErrorOr<EditFisherCommandResponse>> Handle(EditFisherCommand request,
                                                                   CancellationToken cancellationToken)
    {
        Fisher? fisher = await _context.Fishers.FindAsync(request.FisherId, cancellationToken);

        if (fisher is null)
        {
            return Errors.Fishers.NotFound;
        }

        User? user = await _context.Users.FirstOrDefaultAsync(u => u.FisherId == request.FisherId,
                                                                          cancellationToken);

        bool dniExists = await _context.Users.AnyAsync(u => u.DNI == request.NewDNI
                                                            && u.FisherId != request.FisherId,
                                                       cancellationToken);
        if (dniExists)
        {
            return Errors.Users.DNIAlreadyExists;
        }

        fisher.ChangeName(request.NewFirstName, request.NewLastName);

        if (user is not null)
        {
            user.ChangeName(request.NewFirstName, request.NewLastName);

            if (user.ChangeDNI(request.NewDNI) is var ret && ret.IsError)
            {
                return ret.Errors;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new EditFisherCommandResponse(fisher.Id,
                                             user?.FirstName ?? request.NewFirstName,
                                             user?.LastName ?? request.NewLastName,
                                             user?.DNI ?? request.NewDNI);
    }
}
