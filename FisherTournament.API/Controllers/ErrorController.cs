using Microsoft.AspNetCore.Mvc;

namespace FisherTournament.API.Controllers;

public class ErrorController : ControllerBase
{
    [Route("/error")]
    public IActionResult Error() => Problem();
}