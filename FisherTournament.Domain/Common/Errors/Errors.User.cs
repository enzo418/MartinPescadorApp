using ErrorOr;

namespace FisherTournament.Domain.Common.Errors;

public static partial class Errors
{
    public static class Users
    {
        public static Error AlreadyHasFisher => Error.Conflict(
            code: "User.AlreadyHasFisher",
            description: "User already has a fisher.");
    }
}