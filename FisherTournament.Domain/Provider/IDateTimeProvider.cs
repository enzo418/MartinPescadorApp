namespace FisherTournament.Domain.Common.Provider;

public interface IDateTimeProvider
{
    DateTime Now { get; }
}