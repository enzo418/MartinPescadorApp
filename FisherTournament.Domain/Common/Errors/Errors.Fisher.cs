using ErrorOr;
using i8n.Errors.Fisher;

namespace FisherTournament.Domain.Common.Errors;

public static partial class Errors
{
	public static class Fishers
	{
		public static Error NotFound => Error.NotFound(
			code: "Fisher.NotFound",
			description: FisherErrors.NotFound);
	}
}