using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;

namespace FisherTournament.Application.Tournaments.Commands.EditCategory
{
    public record struct EditCategoryCommand(string TournamentId, string CategoryId, string Name)
        : IRequest<ErrorOr<EditCategoryCommandResponse>>;

    public record struct EditCategoryCommandResponse(string Id, string Name);

    public class EditCategoryCommandHandler
        : IRequestHandler<EditCategoryCommand, ErrorOr<EditCategoryCommandResponse>>
    {
        private readonly ITournamentFisherDbContext _context;

        public EditCategoryCommandHandler(ITournamentFisherDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<EditCategoryCommandResponse>> Handle(EditCategoryCommand request, CancellationToken cancellationToken)
        {
            var tournamentId = TournamentId.Create(request.TournamentId);

            if (tournamentId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(request.TournamentId));
            }

            var tournament = await _context.Tournaments
                .FindAsync(new object[] { tournamentId.Value }, cancellationToken: cancellationToken);

            if (tournament == null)
            {
                return Errors.Tournaments.NotFound;
            }

            var categoryId = CategoryId.Create(request.CategoryId);

            if (categoryId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(request.CategoryId));
            }

            if (tournament.EditCategory(categoryId.Value, request.Name) is var res && res.IsError)
            {
                return res.Errors;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return new EditCategoryCommandResponse(categoryId.Value.ToString(), request.Name);
        }
    }
}
