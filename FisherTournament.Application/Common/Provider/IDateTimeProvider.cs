namespace FisherTournament.Application.Common.Provider;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}