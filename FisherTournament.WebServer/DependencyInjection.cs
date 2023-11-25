using FisherTournament.Infrastracture.Common.Mapping;

namespace FisherTournament.WebServer
{
    public static partial class DependencyInjection
    {
        public static IServiceCollection AddWebServer(this IServiceCollection services)
        {
            services.AddMappings();

            return services;
        }
    }
}