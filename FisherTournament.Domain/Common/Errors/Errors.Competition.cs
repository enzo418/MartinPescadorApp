using ErrorOr;
using i8n.Errors.Competition;

namespace FisherTournament.Domain.Common.Errors;

public static partial class Errors
{
	public static class Competitions
	{
		public static Error NotFound => Error.NotFound(
			code: "Competition.NotFound",
			description: CompetitionErrors.NotFound);

		public static Error StartDateBeforeTournament => Error.Conflict(
					code: "Competition.StartDateBeforeTournament",
					description: CompetitionErrors.StartDateBeforeTournament);

		public static Error HasNotStarted => Error.Conflict(
					code: "Competition.HasNotStarted",
					description: CompetitionErrors.HasNotStarted);

		public static Error HasEnded => Error.Conflict(
					code: "Competition.HasEnded",
					description: CompetitionErrors.HasEnded);
	}
}