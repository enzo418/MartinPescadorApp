using FisherTournament.API.Common.CustomResults;
using FisherTournament.Application.Tournaments.Commands.CreateTournament;
using FisherTournament.Contracts.Tournaments;
using MapsterMapper;
using MediatR;
using MinimalApi.Endpoint;

namespace FisherTournament.API.Endpoints.Tournaments;

public class CreateTournamentEndpoint : IEndpoint<IResult, CreateTournamentRequest>
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public CreateTournamentEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapPost("/tournaments", async (CreateTournamentRequest cmd) => await HandleAsync(cmd))
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

    public async Task<IResult> HandleAsync(CreateTournamentRequest request)
    {
        var command = _mapper.Map<CreateTournamentCommand>(request);
        var response = await _sender.Send(command);
        return response.Match(
            success => Results.Ok(_mapper.Map<CreateTournamentResponse>(success)),
            errors => Results.Extensions.Problem(errors));
    }
}