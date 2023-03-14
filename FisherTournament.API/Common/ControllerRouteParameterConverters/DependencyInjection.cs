using System.ComponentModel;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.API.Common.ControllerRouteParameterConverters;

public static class DependencyInjection
{
    public static IServiceCollection AddControllerRouteParameterConverters(this IServiceCollection services)
    {
        AddConverterFor<FisherId>();
        AddConverterFor<TournamentId>();
        AddConverterFor<CompetitionId>();

        // Assembly scan seems an overkill but add it if you want it to be automatic

        return services;
    }

    private static void AddConverterFor<T>()
    {
        TypeDescriptor.AddAttributes(typeof(T), new TypeConverterAttribute(typeof(GuidIdConverter<T>)));
    }

}