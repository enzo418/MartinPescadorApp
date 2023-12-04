using ErrorOr;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Application.Common.Resources;
using FisherTournament.Domain.Common.Errors;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Application.Competitions.Queries.GetCompetition
{
	public record struct GetCompetitionQuery(string CompetitionId)
		: IRequest<ErrorOr<GetCompetitionQueryResponse>>;

	public record struct GetCompetitionQueryResponse(
		string CompetitionId,
		DateTime StartDateTime,
		DateTime? EndDateTime,
		CompetitionLocationResource Location
	);

	public class GetCompetitionQueryHandler
		: IRequestHandler<GetCompetitionQuery, ErrorOr<GetCompetitionQueryResponse>>
	{
		private readonly ITournamentFisherDbContext _context;

		public GetCompetitionQueryHandler(ITournamentFisherDbContext context)
		{
			_context = context;
		}

		public async Task<ErrorOr<GetCompetitionQueryResponse>> Handle(GetCompetitionQuery request, CancellationToken cancellationToken)
		{
			var competitionId = CompetitionId.Create(request.CompetitionId);

			if (competitionId.IsError)
			{
				return Errors.Id.NotValidWithProperty(nameof(request.CompetitionId));
			}

			var competition = await _context.Competitions
				.Where(c => c.Id == competitionId.Value)
				.Select(c => new GetCompetitionQueryResponse(
					c.Id.ToString(),
					c.StartDateTime,
					c.EndDateTime,
					new CompetitionLocationResource(c.Location.City, c.Location.State, c.Location.Country, c.Location.Place)))
				.FirstOrDefaultAsync(cancellationToken);

			if (EqualityComparer<GetCompetitionQueryResponse>.Default.Equals(competition, default))
			{
				return Errors.Competitions.NotFound;
			}

			return competition;
		}
	}

}
