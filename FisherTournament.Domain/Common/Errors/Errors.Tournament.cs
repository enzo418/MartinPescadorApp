using ErrorOr;

namespace FisherTournament.Domain.Common.Errors;

public static partial class Errors
{
    public static class Tournament
    {
        public static Error NotFound => Error.NotFound(
            code: "Tournament.NotFound",
            description: "Tournament not found.");

        public static Error NotEnrolled => Error.Conflict(
            code: "Tournament.NotEnrolled",
            description: "Fisher is not enrolled in the tournament"
        );

        public static Error InscriptionAlreadyExists => Error.Conflict(
            code: "Tournament.InscriptionAlreadyExists",
            description: "Fisher is already enrolled in the tournament"
        );
    }
}