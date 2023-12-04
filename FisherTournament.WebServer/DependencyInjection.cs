using FisherTournament.Infrastracture.Common.Mapping;
using Mapster;

namespace FisherTournament.WebServer
{
	public static partial class DependencyInjection
	{
		public static IServiceCollection AddWebServer(this IServiceCollection services)
		{
			// Set Mapster to compile with debug info - https://github.com/MapsterMapper/Mapster/wiki/Debugging#do-not-worry-about-performance
			/*TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.CompileWithDebugInfo();*/

			// Throw an exception if a mapping is not explicitly defined
			TypeAdapterConfig.GlobalSettings.RequireExplicitMapping = true;

			services.AddMappings();

			return services;
		}
	}
}