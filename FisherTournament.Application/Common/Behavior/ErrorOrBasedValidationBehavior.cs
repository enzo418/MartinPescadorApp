using ErrorOr;
using FisherTournament.Application.Common.Instrumentation;
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
    private readonly ApplicationInstrumentation _instrumentation;

    public ErrorOrBasedValidationBehavior(IEnumerable<IValidator<TRequest>> validators, ApplicationInstrumentation instrumentation)
    {
        _validators = validators;
        _instrumentation = instrumentation;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var activity = _instrumentation.ActivitySource.StartActivity("ValidationBehavior");

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

            activity?.Stop();
            return (dynamic)errors;
        }

        activity?.Stop();

        return await next();
    }
}