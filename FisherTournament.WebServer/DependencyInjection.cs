using FisherTournament.Application.LeaderBoard;
using FisherTournament.Infrastructure.Common.Mapping;
using FisherTournament.WebServer.Services.ExportLeaderboard;
using FisherTournament.WebServer.Services.LeaderboardNotification;
using Mapster;
using System.Linq.Expressions;

namespace FisherTournament.WebServer
{
	public static partial class DependencyInjection
	{
		public static IServiceCollection AddWebServer(this IServiceCollection services)
		{
			// Set Mapster to compile with debug info - https://github.com/MapsterMapper/Mapster/wiki/Debugging#do-not-worry-about-performance
			TypeAdapterConfig.GlobalSettings.Compiler = exp => exp.CompileWithDebugInfo();

			// Throw an exception if a mapping is not explicitly defined
			TypeAdapterConfig.GlobalSettings.RequireExplicitMapping = true;

			services.AddMappings();

			services.AddLeaderboardNotifier();

			services.AddLeaderboardExporter();

			return services;
		}

		private static IServiceCollection AddLeaderboardNotifier(this IServiceCollection services)
		{
			services.AddSingleton<LeaderboardNotificationService>();
			services.AddSingleton<ILeaderboardNotificationClient>(provider => provider.GetRequiredService<LeaderboardNotificationService>());

			return services;
		}

		private static IServiceCollection AddLeaderboardExporter(this IServiceCollection services)
		{
			services.AddTransient<ExportLeaderboardService>();

			return services;
		}
	}
}