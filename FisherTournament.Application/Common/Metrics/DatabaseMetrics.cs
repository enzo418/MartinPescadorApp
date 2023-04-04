using App.Metrics;
using App.Metrics.Timer;

namespace FisherTournament.Application.Common.Metrics
{
    public static partial class ApplicationMetrics
    {
        public class DatabaseMetrics
        {
            public static TimerOptions SaveChangesTimer => new TimerOptions
            {
                Name = "Save_Changes_Timer",
                MeasurementUnit = Unit.Events,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };

            public static TimerOptions SaveChangesBeforeDispatchEventsTimer => new TimerOptions
            {
                Name = "Save_Changes_Dispatch_Events_Before_Timer",
                MeasurementUnit = Unit.Events,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };

            public static TimerOptions SaveChangesAfterDispatchEventsTimer => new TimerOptions
            {
                Name = "Save_Changes_Dispatch_Events_After_Timer",
                MeasurementUnit = Unit.Events,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };
        }
    }
}