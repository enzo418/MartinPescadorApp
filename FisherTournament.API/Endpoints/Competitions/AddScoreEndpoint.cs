using FisherTournament.API.Common.CustomResults;
using FisherTournament.Application.Competitions.Commands.AddScore;
using FisherTournament.Contracts.Competitions;
using MapsterMapper;
using MediatR;
using MinimalApi.Endpoint;

namespace FisherTournament.API.Endpoints.Competitions;

public class AddScoreEndpoint : IEndpoint<IResult, AddScoreRequest, string, ISender>
{
    private readonly IMapper _mapper;

    public AddScoreEndpoint(IMapper mapper)
    {
        _mapper = mapper;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapPost("/tournaments/{tournamentId}/competitions/{competitionId}/scores",
            async (AddScoreRequest cmd,
                   string competitionId,
                   ISender sender) => await HandleAsync(cmd, competitionId, sender))
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithTags("CompetitionEndpoints")
            .WithName("AddScore")
            .WithOpenApi(cfg => new(cfg)
            {
                Summary = "Add a score to a fisher in a competition",
                Description = "Add a score to a fisher in a competition",
            });
    }

    public async Task<IResult> HandleAsync(AddScoreRequest request,
                                           string competitionId,
                                           ISender sender)
    {
        var command = _mapper.Map<AddScoreCommand>((request, competitionId));
        var response = await sender.Send(command);
        return response.Match(
            value => Results.Ok(),
            errors => Results.Extensions.Problem(errors)
        );
    }
}