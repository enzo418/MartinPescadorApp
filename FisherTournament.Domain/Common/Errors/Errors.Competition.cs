using ErrorOr;

namespace FisherTournament.Domain.Common.Errors;

public static partial class Errors
{
    public static class Competitions
    {
        public static Error NotFound => Error.NotFound(
            code: "Competition.NotFound",
            description: "Competition not found.");

        public static Error StartDateBeforeTournament => Error.Conflict(
                    code: "Competition.StartDateBeforeTournament",
                    description: "Competition start date must be after tournament start date.");

        public static Error HasNotStarted => Error.Conflict(
                    code: "Competition.HasNotStarted",
                    description: "Competition has not started yet.");

        public static Error HasEnded => Error.Conflict(
                    code: "Competition.HasEnded",
                    description: "Competition has ended.");
    }
}