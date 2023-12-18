namespace FisherTournament.Infrastructure.Persistence.Common;

using FisherTournament.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Linq.Expressions;

public class GuidIdConverter<T> : ValueConverter<T, Guid> where T : GuidId<T>
{
    public GuidIdConverter()
        : base(x => x.Value, x => GuidId<T>.Create(x).Value)
    {
    }
}

public static partial class Extension
{
    public static PropertyBuilder<T> HasGuidIdConversion<T>(
            this PropertyBuilder<T> propertyBuilder)
        where T : GuidId<T>
    {
        var converter = new ValueConverter<T, Guid>(
            x => x.Value,
            x => GuidId<T>.Create(x).Value);

        propertyBuilder.HasConversion(converter);

        return propertyBuilder;
    }

    public static PropertyBuilder<T?> HasNullableGuidIdConversion<T>(
            this PropertyBuilder<T?> propertyBuilder)
        where T : GuidId<T>
    {
        var converter = new ValueConverter<T?, Guid?>(
            x => x == null ? null : x.Value,
            x => x.HasValue ? GuidId<T>.Create(x.Value).Value : null);

        propertyBuilder.HasConversion(converter);

        return propertyBuilder;
    }

    public static PropertyBuilder HasGuidIdConversion<T>(
            this PropertyBuilder propertyBuilder)
        where T : GuidId<T>
    {
        var converter = new ValueConverter<T, Guid>(
            x => x.Value,
            x => GuidId<T>.Create(x).Value);

        propertyBuilder.HasConversion(converter);

        return propertyBuilder;
    }

    public static void HasGuidIdKey<F, T>(
            this EntityTypeBuilder<F> modelBuilder,
            Expression<Func<F, T>> keyExpression)
        where T : GuidId<T>
        where F : class
    {
        // cast keyExpression to Expression<Func<F, object?>>
        Expression<Func<F, object?>> keyExpression2 = Expression.Lambda<Func<F, object?>>(
            keyExpression.Body,
            keyExpression.Parameters);

        var propertyBuilder = modelBuilder.HasKey(
            keyExpression2);

        var converter = new ValueConverter<T, Guid>(
            x => x.Value,
            x => GuidId<T>.Create(x).Value);

        modelBuilder.Property(keyExpression)
                    .HasConversion(converter)
                    .ValueGeneratedNever();
    }
}

public static partial class Extension
{
    public static PropertyBuilder<T> HasIntIdConversion<T>(
            this PropertyBuilder<T> propertyBuilder)
        where T : IntId<T>
    {
        var converter = new ValueConverter<T, int>(
            x => x.Value,
            x => IntId<T>.Create(x).Value);

        propertyBuilder.HasConversion(converter);

        return propertyBuilder;
    }
}