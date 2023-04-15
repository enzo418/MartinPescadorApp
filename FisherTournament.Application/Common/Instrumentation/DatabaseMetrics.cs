using System.Diagnostics.Metrics;

namespace FisherTournament.Application.Common.Metrics
{
    public static partial class ApplicationMetrics
    {
        public class DatabaseMetrics : IProjectMetrics
        {
            public static string MetricsName = "fishertournament-application-database";

            private static Meter Meter = new(MetricsName, typeof(DatabaseMetrics).Assembly.GetName().Version?.ToString());

            public static HistogramBasedTimer SaveChangesHistogram = new HistogramBasedTimer(
                Meter,
                MetricsName + "-save-changes",
                "ms"
            );
        }
    }
}