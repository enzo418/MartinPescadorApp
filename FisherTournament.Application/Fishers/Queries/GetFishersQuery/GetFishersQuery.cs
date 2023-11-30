using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Application.Common.Requests;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Fishers.Queries
{
	public record struct GetFishersQuery(string? Name, string? DNI, int Page, int PageSize)
		: IRequest<ErrorOr<PagedList<FisherItem>>>, IPagedListQuery;

	public class GetFishersQueryValidator : PagedListQueryValidator<GetFishersQuery> { }

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

			if (string.IsNullOrWhiteSpace(request.Name) && string.IsNullOrWhiteSpace(request.DNI))
			{

				query = _context.Fishers
							.Join(_context.Users, f => f.Id, u => u.FisherId, (f, u) => new { f, u })
							.Select(r => new FisherItem(r.f.Id, r.u.FirstName, r.u.LastName, r.u.DNI));
			} else
			{
				var tmpQuery = _context.Fishers
							.Join(_context.Users, f => f.Id, u => u.FisherId, (f, u) => new { f, u });

				if (!string.IsNullOrWhiteSpace(request.Name))
				{
					tmpQuery = tmpQuery.Where(r => EF.Functions.Like(r.f.Name, $"%{request.Name}%"));
				}

				if (!string.IsNullOrWhiteSpace(request.DNI))
				{
					tmpQuery = tmpQuery.Where(r => EF.Functions.Like(r.u.DNI, $"%{request.DNI}%"));
				}

				query = tmpQuery.Select(r => new FisherItem(r.f.Id, r.u.FirstName, r.u.LastName, r.u.DNI));
			}

			var fishers = await PagedList<FisherItem>.CreateAsync(query, request.Page, request.PageSize);

			return fishers;
		}
	}
}