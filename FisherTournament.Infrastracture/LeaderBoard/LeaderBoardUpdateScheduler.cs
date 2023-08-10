using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.Application.LeaderBoard;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using Microsoft.Extensions.Logging;

namespace FisherTournament.Infrastracture.LeaderBoard;

public enum UpdateType
{
    AllCompetitions,
    CompetitionsFromList
}

// note: quartz recommends only using primitive types the the job data
public record ExtendedJob(TournamentId TournamentId,
                    CategoryId CategoryId,
                    List<CompetitionId> CompetitionsToUpdate,
                    DateTimeOffset ExecuteAt) : Job(TournamentId, CategoryId, CompetitionsToUpdate)
{
    public UpdateType UpdateType => CompetitionsToUpdate.Any() ? UpdateType.CompetitionsFromList : UpdateType.AllCompetitions;
    public bool IsReady => ExecuteAt <= DateTimeOffset.UtcNow;
    internal void SetUpdateType(UpdateType updateType)
    {
        if (updateType == UpdateType.AllCompetitions)
        {
            CompetitionsToUpdate.Clear();
        }
    }
}

/// <summary>
/// The LeaderBoardUpdateScheduler class is responsible for scheduling updates to the tournament leaderboard.
/// </summary>
/// TODO: UNIT TEST
public class LeaderBoardUpdateScheduler : ILeaderBoardUpdateScheduler
{
    /// <summary>
    /// The maximum interval between updates.
    /// </summary>
    /// <remarks>
    /// This is to prevent the scheduler from being overloaded with jobs, 
    /// by combining multiple incoming updates into one.
    /// </remarks>
    public static readonly TimeSpan MaxUpdateInterval = TimeSpan.FromSeconds(5);

    /// <summary>
    /// The interval at which the scheduler checks for jobs that need to be executed.
    /// </summary>
    public static readonly TimeSpan CallInterval = TimeSpan.FromSeconds(1);

    private readonly ILogger<LeaderBoardUpdateScheduler> _logger;

    public LeaderBoardUpdateScheduler(ILogger<LeaderBoardUpdateScheduler> logger)
    {
        _logger = logger;
    }

    private readonly Dictionary<(TournamentId, CategoryId), ExtendedJob> _jobs = new();
    private readonly Dictionary<(TournamentId, CategoryId), DateTimeOffset> _lastUpdate = new();

    private readonly Mutex _mutex = new();

    /// <summary>
    /// Schedule a leaderboard update for a specific tournament, competition, and category.
    /// </summary>
    /// <param name="tournamentId">The ID of the tournament to update.</param>
    /// <param name="competitionId">The ID of the competition to update.</param>
    /// <param name="categoryId">The ID of the category to update.</param>
    public void ScheduleLeaderBoardUpdate(TournamentId tournamentId, CompetitionId competitionId, CategoryId categoryId)
    {
        _mutex.WaitOne();

        if (_jobs.TryGetValue((tournamentId, categoryId), out var job))
        {
            if (job.UpdateType == UpdateType.CompetitionsFromList)
            {
                if (!job.CompetitionsToUpdate.Contains(competitionId))
                {
                    job.CompetitionsToUpdate.Add(competitionId);

                    _logger.LogInformation("Scheduling leaderboard update for job {Job} to also update competition {CompetitionId} in {} seconds. Total competitions to update: {}",
                                           job, competitionId, MaxUpdateInterval.TotalSeconds, job.CompetitionsToUpdate.Count);
                }
                else
                {
                    // else do nothing, we are already updating this competition.
                    _logger.LogInformation("Scheduler did not modify job {Job} to also update competition {CompetitionId} because we are already updating it.",
                                           job, competitionId);
                }
            }
            else
            {
                // else do nothing, we are already updating all competitions.
                _logger.LogInformation("Scheduler did not modify job {Job} to also update competition {CompetitionId} because we are already updating all competitions.",
                                       job, competitionId);
            }
        }
        else
        {
            if (_lastUpdate.TryGetValue((tournamentId, categoryId), out var lastUpdate)
                && lastUpdate >= DateTimeOffset.UtcNow.Subtract(MaxUpdateInterval)) // last update was less than 5s ago
            {
                _logger.LogInformation("Scheduling leaderboard update for tournament {TournamentId}, category {CategoryId} to update competition {CompetitionId} in {} seconds",
                                       tournamentId, categoryId, competitionId, MaxUpdateInterval.TotalSeconds);

                _jobs.TryAdd((tournamentId, categoryId),
                             new ExtendedJob(tournamentId,
                                     categoryId,
                                     new List<CompetitionId> { competitionId },
                                     lastUpdate.Add(MaxUpdateInterval)));
            }
            else
            {
                _logger.LogInformation("Scheduling leaderboard update for tournament {TournamentId}, category {CategoryId} to update competition {CompetitionId} NOW",
                                       tournamentId, categoryId, competitionId);

                _jobs.TryAdd((tournamentId, categoryId),
                             new ExtendedJob(tournamentId,
                                     categoryId,
                                     new List<CompetitionId> { competitionId },
                                     DateTimeOffset.UtcNow));
            }
        }

        _mutex.ReleaseMutex();
    }

    /// <summary>
    /// Schedule a leaderboard update for a specific tournament and category, updating all competitions in that category.
    /// </summary>
    /// <param name="tournamentId">The ID of the tournament to update.</param>
    /// <param name="categoryId">The ID of the category to update.</param>
    public void ScheduleLeaderBoardUpdate(TournamentId tournamentId, CategoryId categoryId)
    {
        _mutex.WaitOne();

        if (_jobs.TryGetValue((tournamentId, categoryId), out var job))
        {
            _logger.LogInformation("Updated job for tournament {TournamentId}, category {CategoryId} to update all competitions",
                                   tournamentId, categoryId);
            job.SetUpdateType(UpdateType.AllCompetitions);
        }
        else
        {
            if (_lastUpdate.TryGetValue((tournamentId, categoryId), out var lastUpdate)
                && lastUpdate >= DateTimeOffset.UtcNow.Subtract(MaxUpdateInterval)) // last update was less than 5s ago
            {
                _logger.LogInformation("Scheduling leaderboard update for tournament {TournamentId}, category {CategoryId} to update all competitions in {} seconds",
                                       tournamentId, categoryId, MaxUpdateInterval.TotalSeconds);

                _jobs.TryAdd((tournamentId, categoryId),
                             new ExtendedJob(tournamentId,
                                     categoryId,
                                     new List<CompetitionId>(),
                                     lastUpdate.Add(MaxUpdateInterval)));
            }
            else
            {
                _logger.LogInformation("Scheduling leaderboard update for tournament {TournamentId}, category {CategoryId} to update all competitions NOW",
                                       tournamentId, categoryId);

                _jobs.TryAdd((tournamentId, categoryId),
                             new ExtendedJob(tournamentId,
                                     categoryId,
                                     new List<CompetitionId>(),
                                     DateTimeOffset.UtcNow));
            }
        }

        _mutex.ReleaseMutex();
    }

    public Job? GetNextJob()
    {
        _logger.LogInformation("Getting next job");

        _mutex.WaitOne();

        // OPTIMIZE: use priority queue, priority = min(0, executeAt - now)
        var job = _jobs.Values.OrderBy(x => x.ExecuteAt).FirstOrDefault(j => j.IsReady);

        if (job is not null)
        {
            _jobs.Remove((job.TournamentId, job.CategoryId), out _);

            _lastUpdate[(job.TournamentId, job.CategoryId)] = job.ExecuteAt;

            _logger.LogInformation("Next job is {Job}", job);
        }
        else
        {
            _logger.LogInformation("No jobs ready");
        }


        _mutex.ReleaseMutex();

        return job as Job;
    }
}