using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Application.Common.Requests;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Tournaments.Queries.GetTournamentInscriptions
{
	public record struct GetTournamentInscriptionsQuery(string TournamentId, string? CategoryId, int Page, int PageSize)
		: IRequest<ErrorOr<PagedList<GetTournamentInscriptionsQueryResult>>>, IPagedListQuery;

	public record struct GetTournamentInscriptionsQueryResult(
		int Number,
		string FisherName,
		string FisherDNI,
		string CategoryName,
		string CategoryId
	);

	/// <summary>
	/// Get all inscriptions for a tournament.
	/// Order by inscription number.
	/// </summary>
	public class GetTournamentInscriptionsQueryHandler
		: IRequestHandler<GetTournamentInscriptionsQuery, ErrorOr<PagedList<GetTournamentInscriptionsQueryResult>>>
	{
		private readonly ITournamentFisherDbContext _context;

		public GetTournamentInscriptionsQueryHandler(ITournamentFisherDbContext context)
		{
			_context = context;
		}

		public async Task<ErrorOr<PagedList<GetTournamentInscriptionsQueryResult>>> Handle(
			GetTournamentInscriptionsQuery request,
			CancellationToken cancellationToken)
		{
			var tournamentId = TournamentId.Create(request.TournamentId);

			if (tournamentId.IsError)
			{
				return Errors.Id.NotValidWithProperty(nameof(request.TournamentId));
			}

			var tournament = await _context.Tournaments
				.Where(t => t.Id == tournamentId.Value)
				.AsNoTracking()
				.FirstOrDefaultAsync(cancellationToken);

			if (tournament == null)
			{
				return Errors.Tournaments.NotFound;
			}

			/* SQL IDEA
			 select f.Name, f.DNI, f.BirthDate, c.Name, c.Id
			 from Tournament t
				join Inscription i on t.Id = i.TournamentId
				join Fisher f on i.FisherId = f.Id
				join Category c on i.CategoryId = c.Id
				where t.Id = @tournamentId
			*/

			IQueryable<GetTournamentInscriptionsQueryResult>? query;

			if (string.IsNullOrEmpty(request.CategoryId))
			{
				query = (
					from t in _context.Tournaments
					from i in t.Inscriptions
					join f in _context.Fishers on i.FisherId equals f.Id
					join u in _context.Users on f.Id equals u.FisherId into mU
					from maybeUser in mU.DefaultIfEmpty()
					from c in t.Categories
					where t.Id == tournamentId.Value && c.Id == i.CategoryId
					orderby i.Number
					select new GetTournamentInscriptionsQueryResult(
							i.Number,
							f.Name,
							maybeUser.DNI,
							c.Name,
							c.Id
					)
				);
			} else
			{
				var categoryId = CategoryId.Create(request.CategoryId);

				if (categoryId.IsError)
				{
					return Errors.Id.NotValidWithProperty(nameof(request.CategoryId));
				}

				query = (
					from t in _context.Tournaments
					from i in t.Inscriptions
					join f in _context.Fishers on i.FisherId equals f.Id
					join u in _context.Users on f.Id equals u.FisherId
					from c in t.Categories
					where t.Id == tournamentId.Value
						&& c.Id == categoryId.Value
						&& c.Id == i.CategoryId
					select new GetTournamentInscriptionsQueryResult(
						i.Number,
						f.Name,
						u.DNI,
						c.Name,
						c.Id
					)
				);
			}

			var inscriptions = await PagedList<GetTournamentInscriptionsQueryResult>.CreateAsync(query, request.Page, request.PageSize);

			return inscriptions;
		}
	}
}
