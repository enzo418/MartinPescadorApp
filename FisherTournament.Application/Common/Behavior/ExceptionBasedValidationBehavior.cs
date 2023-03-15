using FluentValidation;
using MediatR;

namespace FisherTournament.Application.Common.Behavior;

public class ExceptionBasedValidationBehavior<TRequest, TResponse>
 : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
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
            .Select(validator => validator.Validate(request))
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