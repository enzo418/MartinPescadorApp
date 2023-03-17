using FisherTournament.API.Common.CustomResults;
using FisherTournament.Application.Tournaments.Commands.AddCompetitions;
using FisherTournament.Contracts.Competitions;
using MapsterMapper;
using MediatR;
using MinimalApi.Endpoint;

namespace FisherTournament.API.Endpoints.Competitions;

public class AddCompetitionsEndpoint : IEndpoint<IResult, AddCompetitionsRequest, string>
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public AddCompetitionsEndpoint(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    public void AddRoute(IEndpointRouteBuilder app)
    {
        app.MapPost("/tournaments/{tournamentId}/competitions",
            async (AddCompetitionsRequest cmd, string tournamentId) => await HandleAsync(cmd, tournamentId))
            .Produces<AddCompetitionsResponse>()
            .ProducesValidationProblem()
            .WithTags("CompetitionEndpoints")
            .WithName("AddCompetitions")
            .WithOpenApi(cfg => new(cfg)
            {
                Summary = "Add a competition to a tournament",
                Description = "Add a competition to a tournament",
            });
    }

    public async Task<IResult> HandleAsync(AddCompetitionsRequest request, string tournamentId)
    {
        var command = _mapper.Map<AddCompetitionsCommand>((request, tournamentId));
        var response = await _sender.Send(command);
        return response.Match(
            value => Results.Ok(_mapper.Map<AddCompetitionsResponse>(value)),
            errors => Results.Extensions.Problem(errors)
        );
    }
}