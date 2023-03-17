using FisherTournament.Application.Tournaments.Commands.CreateTournament;
using FisherTournament.Contracts.Tournaments;
using MapsterMapper;
using MediatR;
using MinimalApi.Endpoint;

namespace FisherTournament.API.Endpoints.Tournaments;

public class CreateTournamentEndpoint : IEndpoint<CreateTournamentResponse, CreateTournamentRequest>
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
            .WithOpenApi(cfg => new(cfg)
            {
                Summary = "Create a new tournament",
                Description = "Create a new tournament",
            })
            .WithName("CreateTournament");
    }

    public async Task<CreateTournamentResponse> HandleAsync(CreateTournamentRequest request)
    {
        var command = _mapper.Map<CreateTournamentCommand>(request);
        var response = await _sender.Send(command);
        return _mapper.Map<CreateTournamentResponse>(response);
    }
}