using FisherTournament.API.Common.CustomResults;
using FisherTournament.Application.Competitions.Queries.GetLeaderBoard;
using FisherTournament.Contracts.Competitions;
using MapsterMapper;
using MediatR;
using MinimalApi.Endpoint;

namespace FisherTournament.API.Endpoints.Competitions;

using GetLeaderboardResponse = IEnumerable<CompetitionCategoryLeaderBoard>;

public class GetLeaderboardEndpoint : IEndpoint<IResult, string, ISender>
{
    private readonly IMapper _mapper;

    public GetLeaderboardEndpoint(IMapper mapper)
    {
        _mapper = mapper;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("/tournaments/{tournamentId}/competitions/{competitionId}/leaderboard",
            async (string competitionId, ISender sender) => await HandleAsync(competitionId, sender))
            .Produces<GetLeaderboardResponse>()
            .ProducesValidationProblem()
            .WithTags("CompetitionEndpoints")
            .WithName("GetLeaderboard")
            .WithOpenApi(cfg => new(cfg)
            {
                Summary = "Get the leaderboard for a competition",
                Description = "Get the leaderboard for a competition",
            });
    }

    public async Task<IResult> HandleAsync(string competitionId, ISender _sender)
    {
        var response = await _sender.Send(new GetLeaderBoardQuery(competitionId));
        return response.Match(
            value => Results.Ok(_mapper.Map<GetLeaderboardResponse>(value)),
            errors => Results.Extensions.Problem(errors)
        );
    }
}