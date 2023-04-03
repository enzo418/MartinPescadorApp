using FisherTournament.API.Common.CustomResults;
using FisherTournament.Application.Tournaments.Commands.CreateTournament;
using FisherTournament.Contracts.Tournaments;
using MapsterMapper;
using MediatR;
using MinimalApi.Endpoint;

namespace FisherTournament.API.Endpoints.Tournaments;

public class CreateTournamentEndpoint : IEndpoint<IResult, CreateTournamentRequest, ISender>
{
    private readonly IMapper _mapper;

    public CreateTournamentEndpoint(IMapper mapper)
    {
        _mapper = mapper;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapPost("/tournaments", async (CreateTournamentRequest cmd,
                                           ISender sender) => await HandleAsync(cmd, sender))
            .WithTags("TournamentEndpoints")
            .ProducesValidationProblem()
            .Produces<CreateTournamentResponse>()
            .WithOpenApi(cfg => new(cfg)
            {
                Summary = "Create a new tournament",
                Description = "Create a new tournament",
            })
            .WithName("CreateTournament");
    }

    public async Task<IResult> HandleAsync(CreateTournamentRequest request, ISender sender)
    {
        var command = _mapper.Map<CreateTournamentCommand>(request);
        var response = await sender.Send(command);
        return response.Match(
            success => Results.Ok(_mapper.Map<CreateTournamentResponse>(success)),
            errors => Results.Extensions.Problem(errors));
    }
}