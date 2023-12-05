using ErrorOr;
using i8n.Errors.Category;

namespace FisherTournament.Domain.Common.Errors;

public static partial class Errors
{
	public static class Categories
	{
		public static Error NotFound => Error.NotFound(
			code: "Category.NotFound",
			description: CategoryErrors.NotFound);

		public static Error AlreadyExistsWithName => Error.Conflict(
					code: "Category.AlreadyExistsWithName",
					description: CategoryErrors.AlreadyExistsWithName);
	}
}