using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;

namespace FisherTournament.Application.LeaderBoard;

public interface ILeaderBoardUpdater
{
    Task UpdateLeaderBoard(TournamentId tournamentId, CategoryId categoryId, IEnumerable<CompetitionId>? competitionsIds = null, CancellationToken cancellationToken = default);
}
