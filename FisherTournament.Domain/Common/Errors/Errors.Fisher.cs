using ErrorOr;

namespace FisherTournament.Domain.Common.Errors;

public static partial class Errors
{
    public static class Fisher
    {
        public static Error NotFound => Error.NotFound(
            code: "Fisher.NotFound",
            description: "Fisher not found.");
    }
}