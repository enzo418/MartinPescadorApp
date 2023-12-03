using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FisherTournament.Infrastracture.Persistence.Common
{
    class UTCDateTimeValueConverter : ValueConverter<DateTime, DateTime>
    {
        public UTCDateTimeValueConverter()
            : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }

    class UTCNullableDateTimeValueConverter : ValueConverter<DateTime?, DateTime?>
    {
        public UTCNullableDateTimeValueConverter()
            : base(v => v, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v)
        {
        }
    }

    public static partial class Extension
    {
        public static PropertyBuilder<DateTime> HasUTCValueConversion(
                this PropertyBuilder<DateTime> propertyBuilder)
        {
            propertyBuilder.HasConversion(typeof(UTCDateTimeValueConverter));

            return propertyBuilder;
        }

        public static PropertyBuilder<DateTime?> HasUTCValueConversion(
                this PropertyBuilder<DateTime?> propertyBuilder)
        {
            propertyBuilder.HasConversion(typeof(UTCNullableDateTimeValueConverter));

            return propertyBuilder;
        }
    }
}
