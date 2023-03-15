
using FisherTournament.Domain.Common.Provider;

namespace FisherTournament.Infrastracture.Provider;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.UtcNow;
}