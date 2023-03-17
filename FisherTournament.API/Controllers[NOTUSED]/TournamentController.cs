using FisherTournament.Api.Controllers;
using FisherTournament.Application.Tournaments.Commands.AddInscription;
using FisherTournament.Application.Tournaments.Commands.CreateTournament;
using FisherTournament.Contracts.Tournaments;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FisherTournament.API.Controllers;

[Route("tournaments")]
public class TournamentController : ApiController
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public TournamentController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTournamentCommand command)
    {
        var response = await _sender.Send(command);
        return Ok(response);
    }

    // NOTE: If inscriptions become a large part of the business move it to a separate controller.
    [HttpPost("{tournamentId}/inscriptions")]
    public async Task<IActionResult> AddInscription(
        AddInscriptionRequest request,
        string tournamentId)
    {
        var command = _mapper.Map<AddInscriptionCommand>((request, tournamentId));
        var response = await _sender.Send(command);
        return response.Match(
            _ => Ok(),
            errors => Problem(errors)
        );
    }
}
