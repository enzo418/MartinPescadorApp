using ErrorOr;
using i8n.Errors.Tournament;

namespace FisherTournament.Domain.Common.Errors;

public static partial class Errors
{
	public static class Tournaments
	{
		public static Error CompetitionHasEarlierStartDate => Error.Conflict(
			code: "Tournament.CompetitionHasEarlierStartDate",
			description: TournamentErrors.CompetitionHasEarlierStartDate);

		public static Error AlreadyEnded => Error.Conflict(
			code: "Tournament.AlreadyEnded",
			description: TournamentErrors.AlreadyEnded);

		public static Error NotFound => Error.NotFound(
			code: "Tournament.NotFound",
			description: TournamentErrors.NotFound);

		public static Error NotEnrolled => Error.Conflict(
			code: "Tournament.NotEnrolled",
			description: TournamentErrors.NotEnrolled);

		public static Error InscriptionAlreadyExists => Error.Conflict(
			code: "Tournament.InscriptionAlreadyExists",
			description: TournamentErrors.InscriptionAlreadyExists);

		public static Error IsOver => Error.Conflict(
			code: "Tournament.TournamentIsOver",
			description: TournamentErrors.IsOver);

		public static Error InscriptionNumberAlreadyExists => Error.Conflict(
			code: "Tournament.InscriptionNumberAlreadyExists",
			description: TournamentErrors.InscriptionNumberAlreadyExists);

		public static Error InscriptionNotFound => Error.NotFound(
			code: "Tournament.InscriptionNotFound",
			description: TournamentErrors.InscriptionNotFound);

		public static Error FisherHasAlreadyScored => Error.Conflict(
			code: "Tournament.FisherHasAlreadyScored",
			description: TournamentErrors.FisherHasAlreadyScored);
	}
}
