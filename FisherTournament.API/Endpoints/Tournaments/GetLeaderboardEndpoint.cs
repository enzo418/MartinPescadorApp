using FisherTournament.API.Common.CustomResults;
using FisherTournament.Application.Tournaments.Queries.GetTournamentLeaderBoard;
using FisherTournament.Contracts.Competitions;
using FisherTournament.Contracts.Tournaments;
using MapsterMapper;
using MediatR;
using MinimalApi.Endpoint;
using TournamentLeaderBoardCategory = FisherTournament.Contracts.Tournaments.TournamentLeaderBoardCategory;

namespace FisherTournament.API.Endpoints.Tournaments;

using GetTournamentLeaderBoardResponse = IEnumerable<TournamentLeaderBoardCategory>;

public class GetLeaderboardEndpoint : IEndpoint<IResult, string, ISender>
{
    private readonly IMapper _mapper;

    public GetLeaderboardEndpoint(IMapper mapper)
    {
        _mapper = mapper;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapGet("/tournaments/{tournamentId}/leaderboard",
            async (string tournamentId, ISender sender) => await HandleAsync(tournamentId, sender))
            .Produces<GetTournamentLeaderBoardResponse>()
            .ProducesValidationProblem()
            .WithTags("TournamentEndpoints")
            .WithName("GetTournamentLeaderboard")
            .WithOpenApi(cfg => new(cfg)
            {
                Summary = "Get the leaderboard for a tournament",
                Description = "Get the leaderboard for a tournament"
            });
    }

    public async Task<IResult> HandleAsync(string tournamentId, ISender sender)
    {
        var response = await sender.Send(new GetTournamentLeaderBoardQuery(tournamentId));
        return response.Match(
            value => Results.Ok(_mapper.Map<GetTournamentLeaderBoardResponse>(value)),
            errors => Results.Extensions.Problem(errors)
        );
    }
}