using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;

namespace FisherTournament.Application.Common.Metrics
{
    public static partial class ApplicationMetrics
    {
        public class LeaderboardMetrics : IProjectMetrics
        {
            public static string MetricsName = "fishertournament-application-leaderboard";

            private static Meter Meter = new(MetricsName, typeof(DatabaseMetrics).Assembly.GetName().Version?.ToString());

            public static HistogramBasedTimer LeaderboardUpdate = new HistogramBasedTimer(
                Meter,
                MetricsName + "-update", "ms"
            );
        }
    }
}