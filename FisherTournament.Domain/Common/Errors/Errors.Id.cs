using ErrorOr;

namespace FisherTournament.Domain.Common.Errors;

public static partial class Errors
{
    public static class Id
    {
        public static Error GenericNotValid => Error.Validation(
            code: "Id.InvalidId",
            description: "Not a valid ID value.");


        public static Error NotValidWithProperty(string PropertyName) => Error.Validation(
        code: PropertyName,
        description: $"'{PropertyName}' Should be a valid ID value.");

        public static Error NotValidWithType(string Type, string PropertyName) => Error.Validation(
        code: Type + ".InvalidId",
        description: $"'{PropertyName}' Not a valid ID value.");
    }
}