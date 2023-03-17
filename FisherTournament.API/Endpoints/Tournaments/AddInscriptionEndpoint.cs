using ErrorOr;
using FisherTournament.API.Common.CustomResults;
using FisherTournament.Application.Tournaments.Commands.AddInscription;
using FisherTournament.Contracts.Tournaments;
using MapsterMapper;
using MediatR;
using MinimalApi.Endpoint;

namespace FisherTournament.API.Endpoints.Tournaments;

public class AddInscriptionEndpoint : IEndpoint<IResult, AddInscriptionRequest, string>
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public AddInscriptionEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapPost("/tournaments/{tournamentId}/inscriptions",
            async (AddInscriptionRequest cmd, string tournamentId) => await HandleAsync(cmd, tournamentId))
        .WithTags("TournamentEndpoints")
        .WithOpenApi(cfg => new(cfg)
        {
            Summary = "Add a new inscription to a tournament",
            Description = "Add a new inscription to a tournament",
        })
        .WithName("AddInscription")
        .Produces(StatusCodes.Status200OK)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status409Conflict);
    }

    public async Task<IResult> HandleAsync(AddInscriptionRequest request, string tournamentId)
    {
        var command = _mapper.Map<AddInscriptionCommand>((request, tournamentId));
        var response = await _sender.Send(command);
        return response.Match(
            _ => Results.Ok(),
            errors => Results.Extensions.Problem(errors)
        );
    }
}