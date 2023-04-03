using FisherTournament.API.Common.CustomResults;
using FisherTournament.Application.Competitions.Commands.AddParticipation;
using FisherTournament.Application.Competitions.Commands.AddScore;
using FisherTournament.Contracts.Competitions;
using MapsterMapper;
using MediatR;
using MinimalApi.Endpoint;

namespace FisherTournament.API.Endpoints.Competitions;

public class AddParticipationEndpoint : IEndpoint<IResult, AddParticipationRequest, string, ISender>
{
    private readonly IMapper _mapper;

    public AddParticipationEndpoint(IMapper mapper)
    {
        _mapper = mapper;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapPost("/tournaments/{tournamentId}/competitions/{competitionId}/participations",
            async (AddParticipationRequest cmd,
                   string competitionId,
                   ISender sender) => await HandleAsync(cmd, competitionId, sender))
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .WithTags("CompetitionEndpoints")
            .WithName("AddParticipation")
            .WithOpenApi(cfg => new(cfg)
            {
                Summary = "Add a participation to a fisher in a competition",
                Description = "Add a participation to a fisher in a competition",
            });
    }

    public async Task<IResult> HandleAsync(AddParticipationRequest request,
                                           string competitionId,
                                           ISender sender)
    {
        var command = _mapper.Map<AddParticipationCommand>((request, competitionId));
        var response = await sender.Send(command);
        return response.Match(
            value => Results.Ok(),
            errors => Results.Extensions.Problem(errors)
        );
    }
}