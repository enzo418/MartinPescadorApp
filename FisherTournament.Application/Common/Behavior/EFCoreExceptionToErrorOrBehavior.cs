using ErrorOr;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

            var response = Error.Failure().ConvertToErrorOr<TResponse>();

            if (response is not null)
            {
                return (TResponse)response;
            }

            throw ex;
        }
    }
}