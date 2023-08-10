using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;

namespace FisherTournament.Application.Tournaments.Commands.AddInscription;

using Domain.Common.Errors;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.TournamentAggregate;

public record struct AddSimpleInscriptionCommand(
    string TournamentId,
    string FisherFirstName,
    string FisherLastName,
    string CategoryId,
    int Number)
     : IRequest<ErrorOr<Created>>;

public class AddSimpleInscriptionCommandHandler :
    IRequestHandler<AddSimpleInscriptionCommand, ErrorOr<Created>>
{
    private readonly ITournamentFisherDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public AddSimpleInscriptionCommandHandler(ITournamentFisherDbContext context,
                                              IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<ErrorOr<Created>> Handle(AddSimpleInscriptionCommand request,
                                         CancellationToken cancellationToken)
    {
        ErrorOr<TournamentId> tournamentId = TournamentId.Create(request.TournamentId);

        if (tournamentId.IsError)
        {
            return Errors.Id.NotValidWithProperty(nameof(request.TournamentId));
        }

        Tournament? tournament = _context.Set<Tournament>()
            .FirstOrDefault(t => t.Id == tournamentId.Value);

        if (tournament is null)
        {
            return Errors.Tournaments.NotFound;
        }

        ErrorOr<CategoryId> categoryId = CategoryId.Create(request.CategoryId);

        if (categoryId.IsError)
        {
            return Errors.Id.NotValidWithProperty(nameof(request.CategoryId));
        }

        Fisher fisher = Fisher.Create(request.FisherFirstName, request.FisherLastName);

        ErrorOr<Success> result = tournament.AddInscription(fisher.Id,
                                                            categoryId.Value,
                                                            request.Number,
                                                            _dateTimeProvider);

        if (result.IsError)
        {
            return result.Errors;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Created;
    }
}