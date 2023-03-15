using FluentValidation;

namespace FisherTournament.Application.Common.Validators;

public static class DateTimeValidators
{
    public static IRuleBuilderOptions<T, DateTime> IsUtcDateTime<T>(
        this IRuleBuilder<T, DateTime> ruleBuilder)
    {
        return ruleBuilder.Must(x => x.Kind == DateTimeKind.Utc)
            .WithMessage(v => "'{PropertyValue}' must be a UTC DateTime.");
    }
}