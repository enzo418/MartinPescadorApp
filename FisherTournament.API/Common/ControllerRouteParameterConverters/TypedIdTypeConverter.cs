using System.ComponentModel;
using System.Globalization;
using FisherTournament.Domain;

namespace FisherTournament.API.Common.ControllerRouteParameterConverters;

public class GuidIdConverter<T> : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) =>
        sourceType == typeof(string) /*|| sourceType == typeof(int)*/;

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) =>
        destinationType == typeof(string) /* || destinationType == typeof(int)*/;

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value switch
        {
            string s => GuidId<T>.Create(s) ?? throw new ArgumentException($"Cannot convert from {value} to ProductId", nameof(value)),
            null => null,
            _ => throw new ArgumentException($"Cannot convert from {value} to ProductId", nameof(value))
        };
    }

    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string))
        {
            return value switch
            {
                GuidId<T> id => id.ToString(),
                null => null,
                _ => throw new ArgumentException($"Cannot convert {value} to string", nameof(value))
            };
        }

        throw new ArgumentException($"Cannot convert {value ?? "(null)"} to {destinationType}", nameof(destinationType));
    }
}
