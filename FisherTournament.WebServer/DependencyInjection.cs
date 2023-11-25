using FisherTournament.Infrastracture.Common.Mapping;
using FisherTournament.WebServer.Navigation;

namespace FisherTournament.WebServer
{
    public static partial class DependencyInjection
    {
        public static IServiceCollection AddWebServer(this IServiceCollection services)
        {
            services.AddSingleton<NavigationHistory>();

            services.AddMappings();

            return services;
        }
    }
}