using ErrorOr;
using i8n.Errors.User;

namespace FisherTournament.Domain.Common.Errors;

public static partial class Errors
{
	public static class Users
	{
		public static Error DNIAlreadyExists => Error.Conflict(
			code: "User.DNIAlreadyExists",
			description: UserErrors.DNIAlreadyExists);

		public static Error AlreadyHasFisher => Error.Conflict(
			code: "User.AlreadyHasFisher",
			description: UserErrors.AlreadyHasFisher);
	}
}