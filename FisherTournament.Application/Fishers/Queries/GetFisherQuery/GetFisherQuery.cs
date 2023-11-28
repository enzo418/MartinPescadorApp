using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Fishers.Queries
{
    public record struct GetFisherQuery(string FisherId)
        : IRequest<ErrorOr<FisherItem>>;

    public class GetFisherQueryHandler
        : IRequestHandler<GetFisherQuery, ErrorOr<FisherItem>>
    {
        public ITournamentFisherDbContext _context;

        public GetFisherQueryHandler(ITournamentFisherDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<FisherItem>> Handle(GetFisherQuery request, CancellationToken cancellationToken)
        {
            var fisherId = FisherId.Create(request.FisherId);

            if (fisherId.IsError)
            {
                return Errors.Id.NotValidWithProperty(nameof(request.FisherId));
            }

            var result = await _context.Fishers
                                    .Where(f => f.Id == fisherId.Value)
                                    .Join(_context.Users,
                                          f => f.Id,
                                          u => u.FisherId,
                                          (f, u) => new { f, u })
                                    .FirstOrDefaultAsync(cancellationToken);

            if (result is null)
            {
                return Errors.Fishers.NotFound;
            }

            return new FisherItem(result.f.Id, result.u.FirstName, result.u.LastName, result.u.DNI);
        }
    }
}