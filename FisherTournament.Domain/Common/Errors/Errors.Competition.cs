using ErrorOr;

namespace FisherTournament.Domain.Common.Errors;

public static partial class Errors
{
    public static class Competition
    {
        public static Error NotFound => Error.NotFound(
            code: "Competition.NotFound",
            description: "Competition not found.");
    }
}