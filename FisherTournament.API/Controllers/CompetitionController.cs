using FisherTournament.Application.Competitions.Commands.AddScore;
using FisherTournament.Application.Competitions.Queries.GetLeaderBoard;
using FisherTournament.Application.Tournaments.Commands.AddCompetitions;
using FisherTournament.Contracts.Competitions;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FisherTournament.API.Controllers;

[ApiController]
[Route("tournaments/{tournamentId}/competitions")]
public class CompetitionController : ControllerBase
{

    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public CompetitionController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        AddCompetitionsRequest request,
        TournamentId tournamentId)
    {
        var command = _mapper.Map<AddCompetitionsCommand>((request, tournamentId));

        var response = await _sender.Send(command);

        return Ok(_mapper.Map<AddCompetitionsResponse>(response));
    }

    [HttpPost("{competitionId}/scores")]
    public async Task<IActionResult> AddScore(
        AddScoreRequest request,
        [FromRoute] CompetitionId competitionId)
    {
        var command = _mapper.Map<AddScoreCommand>((request, competitionId));
        await _sender.Send(command);
        return Ok();
    }

    [HttpGet("{competitionId}/Leaderboard")]
    public async Task<IActionResult> GetLeaderboard(CompetitionId competitionId)
    {
        throw new NotImplementedException();
        var response = await _sender.Send(new GetLeaderBoardQuery(competitionId));
        return Ok(response);
    }
}
