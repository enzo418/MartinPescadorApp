using ErrorOr;
using i8n.Errors.Id;

namespace FisherTournament.Domain.Common.Errors;

public static partial class Errors
{
	public static class Id
	{
		public static Error GenericNotValid => Error.Validation(
			code: "Id.InvalidId",
			description: IdErrors.GenericNotValid);


		public static Error NotValidWithProperty(string PropertyName) => Error.Validation(
		code: PropertyName,
		description: IdErrors.NotValidWithProperty.Replace("{PropertyName}", PropertyName));

		public static Error NotValidWithType(string Type, string PropertyName) => Error.Validation(
		code: Type + ".InvalidId",
		description: IdErrors.NotValidWithType.Replace("{PropertyName}", PropertyName));
	}
}