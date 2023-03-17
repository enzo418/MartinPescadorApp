using ErrorOr;
using FluentValidation;
using MediatR;

namespace FisherTournament.Application.Common.Behavior;

public class ErrorOrBasedValidationBehavior<TRequest, TResponse>
 : IPipelineBehavior<TRequest, TResponse>
    where TResponse : IErrorOr

    // Now we are sure it will use IRequest<Y> because it will
    // result in an ErrorOr<T>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ErrorOrBasedValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validationFailures = _validators
            .Select(validator => validator.Validate(request))
            .SelectMany(validationResult => validationResult.Errors)
            .Where(validationFailure => validationFailure != null)
            .ToList();

        if (validationFailures.Any())
        {
            var errors = validationFailures
                .ConvertAll(failure => Error.Validation(
                    code: failure.PropertyName,
                    description: failure.ErrorMessage
                ));
            return (dynamic)errors;
        }

        return await next();
    }
}