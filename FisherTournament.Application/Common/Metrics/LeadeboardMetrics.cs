using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Metrics;
using App.Metrics.Counter;
using App.Metrics.Timer;

namespace FisherTournament.Application.Common.Metrics
{
    public static partial class ApplicationMetrics
    {
        public class LeadeboardMetrics
        {
            public static TimerOptions CompetitionLeadeboardUpdateTimer => new TimerOptions
            {
                Name = "CompetitionLeaderboardUpdateTimer",
                MeasurementUnit = Unit.Events,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };

            public static TimerOptions TournamentLeadeboardUpdateTimer => new TimerOptions
            {
                Name = "TournamentLeaderboardUpdateTimer",
                MeasurementUnit = Unit.Events,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };

            public static CounterOptions LeaderboardUpdateCounter => new CounterOptions
            {
                Name = "LeaderboardUpdateCounter",
                MeasurementUnit = Unit.Events
            };
        }
    }
}