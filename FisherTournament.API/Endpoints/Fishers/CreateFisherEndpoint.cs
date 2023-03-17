using FisherTournament.Application.Fishers.Commands.CreateFisher;
using FisherTournament.Contracts.Fishers;
using MapsterMapper;
using MediatR;
using MinimalApi.Endpoint;

namespace FisherTournament.API.Endpoints.Fishers;

public class CreateFisherEndpoint
 : IEndpoint<CreateFisherResponse, CreateFisherRequest>
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public CreateFisherEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapPost("/fishers", async (CreateFisherRequest cmd) => await HandleAsync(cmd))
            .WithTags("FisherEndpoints")
            .ProducesValidationProblem()
            .WithOpenApi(cfg => new(cfg)
            {
                Summary = "Create a new fisher",
                Description = "Create a new fisher",
            })
            .WithName("CreateFisher");
    }

    public async Task<CreateFisherResponse> HandleAsync(CreateFisherRequest request)
    {
        var command = _mapper.Map<CreateFisherCommand>(request);
        var response = await _sender.Send(command);
        return _mapper.Map<CreateFisherResponse>(response);
    }
}