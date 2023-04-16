using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.Application.LeaderBoard
{
    public record Job(TournamentId TournamentId,
                    CategoryId CategoryId,
                    List<CompetitionId> CompetitionsToUpdate);

    public interface ILeaderBoardUpdateScheduler
    {
        /// <summary>
        /// Updates the competition leader board then updates the tournament leader board.
        /// Only for the given category.
        /// </summary>
        /// <param name="tournamentId"></param>
        /// <param name="competitionId"></param>
        /// <param name="categoryId"></param>
        void ScheduleLeaderBoardUpdate(TournamentId tournamentId, CompetitionId competitionId, CategoryId categoryId);

        /// <summary>
        /// Updates all the competition leader boards then updates the tournament leader board.
        /// Only for the given category.
        /// </summary>
        /// <param name="tournamentId"></param>
        void ScheduleLeaderBoardUpdate(TournamentId tournamentId, CategoryId categoryId);

        /// <summary>
        /// Get the next job that is ready to be executed, removing it from the job dictionary.
        /// </summary>
        /// <returns>A Job object representing the next job to be executed, or null if there are no jobs ready.</returns>
        Job? GetNextJob();
    }
}