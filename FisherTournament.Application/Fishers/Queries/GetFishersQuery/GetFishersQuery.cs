using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Application.Common.Requests;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using MediatR;

namespace FisherTournament.Application.Fishers.Queries
{
    public record struct GetFishersQuery(string? Name, int Page, int PageSize)
        : IRequest<ErrorOr<PagedList<FisherItem>>>, IPagedListQuery;

    public class GetFishersQueryValidator : PagedListQueryValidator<GetFishersQuery> { }

    public record struct FisherItem(FisherId Id, string Name);

    public class GetFishersQueryHandler
        : IRequestHandler<GetFishersQuery, ErrorOr<PagedList<FisherItem>>>
    {
        public ITournamentFisherDbContext _context;

        public GetFishersQueryHandler(ITournamentFisherDbContext context)
        {
            _context = context;
        }

        public async Task<ErrorOr<PagedList<FisherItem>>> Handle(GetFishersQuery request, CancellationToken cancellationToken)
        {
            IQueryable<FisherItem> query;

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = _context.Fishers
                            .Where(f => f.Name.Contains(request.Name))
                            .Select(f => new FisherItem(f.Id, f.Name));
            } else
            {
                query = _context.Fishers
                            .Select(f => new FisherItem(f.Id, f.Name));
            }

            var fishers = await PagedList<FisherItem>.CreateAsync(query, request.Page, request.PageSize);

            return fishers;
        }
    }
}