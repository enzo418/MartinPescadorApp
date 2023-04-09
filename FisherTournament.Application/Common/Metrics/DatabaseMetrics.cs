using App.Metrics;
using App.Metrics.Timer;

namespace FisherTournament.Application.Common.Metrics
{
    public static partial class ApplicationMetrics
    {
        public class DatabaseMetrics
        {
            public static TimerOptions SaveChangesTournamentDbTimer => new TimerOptions
            {
                Name = "Save_Changes_TournamentDb",
                MeasurementUnit = Unit.Events,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };

            public static TimerOptions SaveChangesReadModelsDbTimer => new TimerOptions
            {
                Name = "Save_Changes_ReadModelsDb",
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