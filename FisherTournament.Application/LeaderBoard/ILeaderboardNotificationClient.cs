namespace FisherTournament.Application.LeaderBoard
{
    public interface ILeaderboardNotificationClient
    {
        /// <summary>
        /// Called when the leaderboard is updated.
        /// </summary>
        /// <param name="TournamentId"></param>
        /// <param name="CategoryId"></param>
        /// <param name="CompetitionsId"></param>
        /// <returns></returns>
        Task OnLeaderboardUpdated(string TournamentId, string CategoryId, IEnumerable<string>? CompetitionsId);
    }
}
