using ErrorOr;

namespace FisherTournament.Domain.Common.Errors;

public static partial class Errors
{
    public static class Categories
    {
        public static Error NotFound => Error.NotFound(
            code: "Category.NotFound",
            description: "Category not found.");

        public static Error AlreadyExists => Error.Conflict(
                    code: "Category.AlreadyExists",
                    description: "Category already exists.");
    }
}