using FisherTournament.Api.Controllers;
using FisherTournament.Application.Fishers.Commands.CreateFisher;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FisherTournament.API.Controllers;

[Route("[controller]")]
public class FisherController : ApiController
{

    private readonly ISender _sender;

    public FisherController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateFisherCommand command)
    {
        // CreateFisherCommand or CreateFisherRequest?
        // Request if:
        //      - The controller needs to add some context to the command
        //        (Software Version, some key, or others).
        //      - The body needs to have custom fields that the command doesn't have.
        //      - The body fields have different names that the command.
        //      - If the route takes a query parameter and the command uses it.

        var response = await _sender.Send(command);
        return Ok(response);
    }
}
