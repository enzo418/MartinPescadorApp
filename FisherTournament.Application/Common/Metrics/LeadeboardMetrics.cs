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
        public class LeaderboardMetrics
        {
            public static TimerOptions CompetitionLeaderboardUpdate => new TimerOptions
            {
                Name = "Leaderboard_Competition_Update",
                MeasurementUnit = Unit.Events,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };

            public static TimerOptions TournamentLeaderboardUpdate => new TimerOptions
            {
                Name = "Leaderboard_Tournament_Update",
                MeasurementUnit = Unit.Events,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };

            public static TimerOptions LeaderboardUpdate => new TimerOptions
            {
                Name = "Leaderboard_Update",
                MeasurementUnit = Unit.Events,
                DurationUnit = TimeUnit.Milliseconds,
                RateUnit = TimeUnit.Milliseconds
            };
        }
    }
}