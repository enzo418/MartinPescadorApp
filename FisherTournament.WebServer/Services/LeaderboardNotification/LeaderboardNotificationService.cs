using FisherTournament.Application.LeaderBoard;

namespace FisherTournament.WebServer.Services.LeaderboardNotification
{
    public record struct LeaderboardUpdatedEventArgs(string TournamentId, string CategoryId, IEnumerable<string>? CompetitionIds);
    public delegate Task LeaderboardUpdateHandler(LeaderboardUpdatedEventArgs args);

    /// <summary>
    /// Wrapper of the <see cref="ILeaderboardNotificationClient"/> to be used by the
    /// </summary>
    public sealed class LeaderboardNotificationService : ILeaderboardNotificationClient
    {
        public event LeaderboardUpdateHandler? OnLeaderboardWasUpdated;

        public Task OnLeaderboardUpdated(string TournamentId, string CategoryId, IEnumerable<string>? CompetitionsId)
        {
            if (OnLeaderboardWasUpdated != null)
            {
                var tasks = OnLeaderboardWasUpdated.GetInvocationList()
                    .Cast<LeaderboardUpdateHandler>()
                    .Select(handler => handler.Invoke(new LeaderboardUpdatedEventArgs(TournamentId, CategoryId, CompetitionsId)));

                return Task.WhenAll(tasks);
            }

            return Task.CompletedTask;
        }
    }
}
