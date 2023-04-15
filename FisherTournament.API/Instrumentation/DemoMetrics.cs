using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.Application.Common.Metrics;

namespace FisherTournament.API.Instrumentation
{
    public class DemoMetrics : IProjectMetrics
    {
        public static string MetricsName = "FisherTournament.API." + nameof(DemoMetrics);

        private static Meter Meter = new(MetricsName, typeof(DemoMetrics).Assembly.GetName().Version?.ToString());

        public static HistogramBasedTimer DemoTimer = new HistogramBasedTimer(
            Meter,
            MetricsName + "DemoTimer",
            "ms"
        );

    }
}