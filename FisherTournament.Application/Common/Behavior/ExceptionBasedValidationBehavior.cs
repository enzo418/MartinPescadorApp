using FluentValidation;
using MediatR;

namespace FisherTournament.Application.Common.Behavior;

public class ExceptionBasedValidationBehavior<TRequest, TResponse>
 : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IBaseRequest
    // Both IRequest and IRequest<T> implement IBaseRequest
    // but IRequest<T> does not implement IRequest, which
    // won't work with Command : IRequest<Result>.
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ExceptionBasedValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validationFailures = _validators
            .Select(validator =>
            {
                Console.WriteLine($"Validating {request.GetType().Name} with {validator.GetType().Name}");
                return validator.Validate(request);
            })
            .SelectMany(validationResult => validationResult.Errors)
            .Where(validationFailure => validationFailure != null)
            .ToList();

        if (validationFailures.Any())
        {
            var error = string.Join("\r\n", validationFailures);
            throw new ApplicationException(error);
        }

        return next();
    }
}