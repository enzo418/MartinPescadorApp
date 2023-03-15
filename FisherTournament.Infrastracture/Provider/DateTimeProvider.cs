using FisherTournament.Application.Common.Provider;

namespace FisherTournament.Infrastracture.Provider;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}