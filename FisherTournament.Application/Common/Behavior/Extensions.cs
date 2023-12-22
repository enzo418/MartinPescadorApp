using ErrorOr;
using System.Reflection;

namespace FisherTournament.Application.Common.Behavior
{
    internal static class Extensions
    {
        public static object? ConvertToErrorOr<TResponse>(this Error error)
            where TResponse : IErrorOr
        {
            // Get the static From method using reflection
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(ErrorOr<>))
            {
                Type tErrorOr = typeof(ErrorOr<>).MakeGenericType(typeof(TResponse).GetGenericArguments());
                MethodInfo? mFrom = tErrorOr.GetMethod("From", BindingFlags.Public | BindingFlags.Static);

                if (mFrom is not null)
                {
                    try
                    {
                        object? result = mFrom.Invoke(null, new object[] { new List<Error>() { error } });

                        return result;
                    } catch { }
                }
            }

            return null;
        }
    }
}
