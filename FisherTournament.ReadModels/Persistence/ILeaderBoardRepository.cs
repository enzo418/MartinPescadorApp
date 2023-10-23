using ErrorOr;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.ReadModels.Models;

namespace FisherTournament.ReadModels.Persistence;

public record TournamentCategoryLbCalculatedItem(
    FisherId FisherId,
    int PositionsSum,
    int TotalScore);

// TODO: refactor into single repositories for tournaments and competitions
public interface ILeaderBoardRepository
{
    List<LeaderboardCompetitionCategoryItem> GetCompetitionCategoryLeaderBoard(CompetitionId competitionId,
                                                                               CategoryId categoryId);

    void UpdateCompetitionLeaderBoardItem(LeaderboardCompetitionCategoryItem item);

    void AddCompetitionLeaderBoardItem(LeaderboardCompetitionCategoryItem leaderBoardItem);

    List<LeaderboardTournamentCategoryItem> GetTournamentCategoryLeaderBoard(TournamentId tournamentId,
                                                                             CategoryId categoryId);

    void UpdateTournamentLeaderBoardItem(LeaderboardTournamentCategoryItem item);

    /// <summary>
    /// Calculates the new leader board for a tournament category
    /// </summary>
    /// <remarks>
    /// The leader board is calculated by summing the positions of the fishers in the competitions of the tournament,
    /// and then sorting in ascending order by the sum of the positions.
    /// </remarks>
    /// <param name="tournamentId"></param>
    /// <param name="categoryId"></param>
    /// <param name="tournamentCompetitionsId"></param>
    List<TournamentCategoryLbCalculatedItem> CalculateTournamentCategoryLeaderBoard(TournamentId tournamentId,
                                                                                    CategoryId categoryId,
                                                                                    List<CompetitionId> tournamentCompetitionsId);


    /// <summary>
    /// Gets the positions of the fishers in these competitions
    /// </summary>
    /// <param name="competitionsId"></param>
    /// <param name="categoryId"></param>
    /// <returns></returns>
    Dictionary<FisherId, List<(CompetitionId, int)>> GetFisherCompetitionPositions(List<CompetitionId> competitionsId,
                                                                                    CategoryId categoryId);

    void AddTournamentLeaderBoardItem(LeaderboardTournamentCategoryItem leaderboardTournamentCategoryItem);

    IEnumerable<LeaderboardCompetitionCategoryItem> GetCompetitionLeaderBoard(CompetitionId competitionId);

    IEnumerable<LeaderboardTournamentCategoryItem> GetTournamentLeaderBoard(TournamentId tournamentIdValue);
}