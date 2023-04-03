using FisherTournament.API.Common.CustomResults;
using FisherTournament.Application.Fishers.Commands.CreateFisher;
using FisherTournament.Contracts.Fishers;
using MapsterMapper;
using MediatR;
using MinimalApi.Endpoint;

namespace FisherTournament.API.Endpoints.Fishers;

public class CreateFisherEndpoint
 : IEndpoint<IResult, CreateFisherRequest, ISender>
{
    private readonly IMapper _mapper;

    public CreateFisherEndpoint(IMapper mapper)
    {
        _mapper = mapper;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapPost("/fishers", async (CreateFisherRequest cmd, ISender sender) => await HandleAsync(cmd, sender))
            .WithTags("FisherEndpoints")
            .ProducesValidationProblem()
            .Produces<CreateFisherResponse>()
            .WithOpenApi(cfg => new(cfg)
            {
                Summary = "Create a new fisher",
                Description = "Create a new fisher",
            })
            .WithName("CreateFisher");
    }

    public async Task<IResult> HandleAsync(CreateFisherRequest request, ISender sender)
    {
        var command = _mapper.Map<CreateFisherCommand>(request);
        var response = await sender.Send(command);
        return response.Match(
            success => Results.Ok(_mapper.Map<CreateFisherResponse>(success)),
            error => Results.Extensions.Problem(error));
    }
}