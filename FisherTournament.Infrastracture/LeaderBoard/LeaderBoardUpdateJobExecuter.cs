using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.Application.LeaderBoard;
using Microsoft.Extensions.Logging;
using Quartz;

namespace FisherTournament.Infrastracture.LeaderBoard
{
    public class LeaderBoardUpdateJobExecuter : IJob
    {
        private readonly ILeaderBoardUpdateScheduler _leaderBoardUpdateScheduler;
        private readonly ILeaderBoardUpdater _leaderBoardUpdater;
        private readonly ILogger<LeaderBoardUpdateJobExecuter> _logger;

        public LeaderBoardUpdateJobExecuter(ILeaderBoardUpdateScheduler leaderBoardUpdateScheduler, ILeaderBoardUpdater leaderBoardUpdater, ILogger<LeaderBoardUpdateJobExecuter> logger)
        {
            _leaderBoardUpdateScheduler = leaderBoardUpdateScheduler;
            _leaderBoardUpdater = leaderBoardUpdater;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var job = _leaderBoardUpdateScheduler.GetNextJob();

            if (job == null)
            {
                return;
            }

            _logger.LogInformation("Executing job {job}", job);

            await _leaderBoardUpdater.UpdateLeaderBoard(job.TournamentId, job.CategoryId, job.CompetitionsToUpdate);
        }
    }
}