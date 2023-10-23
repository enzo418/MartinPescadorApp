using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.WebServer.Navigation;
using FisherTournament.Infrastracture.Common.Mapping;

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