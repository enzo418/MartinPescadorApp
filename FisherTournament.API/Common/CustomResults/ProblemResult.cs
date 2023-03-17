using ErrorOr;

namespace FisherTournament.API.Common.CustomResults;

static class ResultsExtensions
{
    public static IResult Problem(this IResultExtensions resultExtensions, Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(statusCode: statusCode, title: error.Description);
    }

    public static IResult Problem(this IResultExtensions resultExtensions, List<Error> errors)
    {
        ArgumentNullException.ThrowIfNull(resultExtensions);

        if (errors.Count == 0)
        {
            return Results.Problem();
        }

        if (errors.All(error => error.Type == ErrorType.Validation))
        {
            return ValidationProblem(errors);
        }

        return ResultsExtensions.Problem(resultExtensions, errors.First());
    }

    private static IResult ValidationProblem(List<Error> errors)
    {
        IDictionary<string, string[]> validationErrors = new Dictionary<string, string[]>();

        foreach (var error in errors)
        {
            validationErrors.Add(error.Code, new[] { error.Description });
        }

        return Results.ValidationProblem(validationErrors);
    }
}