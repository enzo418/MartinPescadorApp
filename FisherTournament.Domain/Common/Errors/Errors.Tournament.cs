using ErrorOr;

namespace FisherTournament.Domain.Common.Errors;

public static partial class Errors
{
    public static class Tournaments
    {
        public static readonly Error CompetitionHasEarlierStartDate = Error.Conflict(
            code: "Tournament.CompetitionHasEarlierStartDate",
            description: "Competition has earlier start date.");

        public static readonly Error AlreadyEnded = Error.Conflict(
            code: "Tournament.AlreadyEnded",
            description: "Tournament already ended.");

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

        public static Error IsOver => Error.Conflict(
            code: "Tournament.TournamentIsOver",
            description: "Tournament is over"
        );

        public static Error InscriptionNumberAlreadyExists => Error.Conflict(
            code: "Tournament.InscriptionNumberAlreadyExists",
            description: "Inscription number already exists");

        public static Error InscriptionNotFound => Error.NotFound(
            code: "Tournament.InscriptionNotFound",
            description: "Inscription not found");
    }
}