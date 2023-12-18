
using FisherTournament.Domain.Common.Provider;

namespace FisherTournament.Infrastructure.Provider;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.UtcNow;
}