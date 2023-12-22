using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace FisherTournament.Application.Common.Behavior;

public class EFCoreExceptionToErrorOrBehavior<TRequest, TResponse>
 : IPipelineBehavior<TRequest, TResponse>
    where TResponse : IErrorOr
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<EFCoreExceptionToErrorOrBehavior<TRequest, TResponse>> _logger;

    public EFCoreExceptionToErrorOrBehavior(ILogger<EFCoreExceptionToErrorOrBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        } catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error while executing request {Request}", request);

            // Get the static From method using reflection
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(ErrorOr<>))
            {
                Type tErrorOr = typeof(ErrorOr<>).MakeGenericType(typeof(TResponse).GetGenericArguments());
                MethodInfo? mFrom = tErrorOr.GetMethod("From", BindingFlags.Public | BindingFlags.Static);

                if (mFrom != null)
                {
                    try
                    {
                        object? result = mFrom.Invoke(null, new object[] { new List<Error>() { Error.Failure() } });

                        if (result is not null)
                        {
                            return (TResponse)result;
                        }
                    } catch { }
                }
            }

            throw ex;
        }
    }
}