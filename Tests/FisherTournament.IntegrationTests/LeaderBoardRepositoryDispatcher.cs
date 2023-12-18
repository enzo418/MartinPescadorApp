using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.ReadModels.Models;
using FisherTournament.ReadModels.Persistence;
using Moq;

namespace FisherTournament.IntegrationTests;

public class LeaderBoardRepositoryDispatcher : ILeaderBoardRepository
{
    public Mock<ILeaderBoardRepository>? RepositoryMock;
    private readonly ILeaderBoardRepository _repository;

    public LeaderBoardRepositoryDispatcher(Mock<ILeaderBoardRepository>? repositoryMock, ILeaderBoardRepository repository)
    {
        this.RepositoryMock = repositoryMock;
        this._repository = repository;
    }

    private ILeaderBoardRepository CurrentRepository
    {
        get
        {
            return RepositoryMock is null ? this._repository : RepositoryMock.Object;
        }
    }

    public void AddCompetitionLeaderBoardItem(LeaderboardCompetitionCategoryItem leaderBoardItem)
    {
        CurrentRepository.AddCompetitionLeaderBoardItem(leaderBoardItem);
    }

    public void AddTournamentLeaderBoardItem(LeaderboardTournamentCategoryItem leaderboardTournamentCategoryItem)
    {
        CurrentRepository.AddTournamentLeaderBoardItem(leaderboardTournamentCategoryItem);
    }

    public List<TournamentCategoryLbCalculatedItem> CalculateTournamentCategoryLeaderBoard(TournamentId tournamentId, CategoryId categoryId, List<CompetitionId> tournamentCompetitionsId)
    {
        return CurrentRepository.CalculateTournamentCategoryLeaderBoard(tournamentId, categoryId, tournamentCompetitionsId);
    }

    public List<LeaderboardCompetitionCategoryItem> GetCompetitionCategoryLeaderBoard(CompetitionId competitionId, CategoryId categoryId)
    {
        return CurrentRepository.GetCompetitionCategoryLeaderBoard(competitionId, categoryId);
    }

    public IEnumerable<LeaderboardCompetitionCategoryItem> GetCompetitionLeaderBoard(CompetitionId competitionId)
    {
        return CurrentRepository.GetCompetitionLeaderBoard(competitionId);
    }

    public Dictionary<FisherId, List<(CompetitionId, int)>> GetFisherCompetitionPositions(List<CompetitionId> competitionsId, CategoryId categoryId)
    {
        return CurrentRepository.GetFisherCompetitionPositions(competitionsId, categoryId);
    }

    public List<LeaderboardTournamentCategoryItem> GetTournamentCategoryLeaderBoard(TournamentId tournamentId, CategoryId categoryId)
    {
        return CurrentRepository.GetTournamentCategoryLeaderBoard(tournamentId, categoryId);
    }

    public IEnumerable<LeaderboardTournamentCategoryItem> GetTournamentLeaderBoard(TournamentId tournamentIdValue)
    {
        return CurrentRepository.GetTournamentLeaderBoard(tournamentIdValue);
    }

    public void RemoveFisherFromLeaderboardCategory(TournamentId tournamentId, IEnumerable<CompetitionId> tournamentCompetitions, CategoryId categoryId, FisherId fisherId)
    {
        CurrentRepository.RemoveFisherFromLeaderboardCategory(tournamentId, tournamentCompetitions, categoryId, fisherId);
    }

    public void UpdateCompetitionLeaderBoardItem(LeaderboardCompetitionCategoryItem item)
    {
        CurrentRepository.UpdateCompetitionLeaderBoardItem(item);
    }

    public void UpdateTournamentLeaderBoardItem(LeaderboardTournamentCategoryItem item)
    {
        CurrentRepository.UpdateTournamentLeaderBoardItem(item);
    }
}
