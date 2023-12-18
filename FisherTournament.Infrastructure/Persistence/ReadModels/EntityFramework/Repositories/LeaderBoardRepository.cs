using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.ReadModels.Models;
using FisherTournament.ReadModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Infrastructure.Persistence.ReadModels.EntityFramework.Repositories
{
    public class LeaderBoardRepository : ILeaderBoardRepository
    {
        private readonly ReadModelsDbContext _dbContext;

        public LeaderBoardRepository(ReadModelsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddCompetitionLeaderBoardItem(LeaderboardCompetitionCategoryItem leaderBoardItem)
        {
            leaderBoardItem.Id = null;
            _dbContext.LeaderboardCompetitionCategoryItems.Add(leaderBoardItem);
        }

        public void AddTournamentLeaderBoardItem(LeaderboardTournamentCategoryItem leaderboardTournamentCategoryItem)
        {
            leaderboardTournamentCategoryItem.Id = null;
            _dbContext.LeaderboardTournamentCategoryItems.Add(leaderboardTournamentCategoryItem);
        }

        public IEnumerable<LeaderboardCompetitionCategoryItem> GetCompetitionLeaderBoard(CompetitionId competitionId)
        {
            return _dbContext.LeaderboardCompetitionCategoryItems
                .Where(x => x.CompetitionId == competitionId)
                .OrderBy(x => x.Position)
                .AsNoTracking()
                .ToList();
        }

        public IEnumerable<LeaderboardTournamentCategoryItem> GetTournamentLeaderBoard(TournamentId tournamentIdValue)
        {
            return _dbContext.LeaderboardTournamentCategoryItems
                .Where(x => x.TournamentId == tournamentIdValue)
                .OrderBy(x => x.Position)
                .AsNoTracking()
                .ToList();
        }

        public List<TournamentCategoryLbCalculatedItem> CalculateTournamentCategoryLeaderBoard(TournamentId tournamentId,
                                                                                               CategoryId categoryId,
                                                                                               List<CompetitionId> tournamentCompetitionsId)
        {
            return _dbContext.LeaderboardCompetitionCategoryItems
                 .Where(x => tournamentCompetitionsId.Contains(x.CompetitionId) && x.CategoryId == categoryId)
                 .GroupBy(x => x.FisherId)
                 .OrderBy(x => x.Sum(y => y.Position))
                 .Select(x => new TournamentCategoryLbCalculatedItem(x.Key,
                                                                     x.Sum(y => y.Position),
                                                                     x.Sum(y => y.Score)))
                 .ToList();
        }

        public Dictionary<FisherId, List<(CompetitionId, int)>> GetFisherCompetitionPositions(List<CompetitionId> competitionsId,
                                                                                    CategoryId categoryId)
        {
            return _dbContext.LeaderboardCompetitionCategoryItems
                .Where(x => competitionsId.Contains(x.CompetitionId) && x.CategoryId == categoryId)
                .Select(x => new
                {
                    x.FisherId,
                    x.CompetitionId,
                    x.Position
                })
                .ToList()
                .GroupBy(x => x.FisherId, s => (s.CompetitionId, s.Position))
                .ToDictionary(g => g.Key, g => g.ToList());
        }

        public List<LeaderboardCompetitionCategoryItem> GetCompetitionCategoryLeaderBoard(CompetitionId competitionId,
                                                                                          CategoryId categoryId)
        {
            return _dbContext.LeaderboardCompetitionCategoryItems
                .Where(x => x.CompetitionId == competitionId && x.CategoryId == categoryId)
                .ToList();
        }

        public List<LeaderboardTournamentCategoryItem> GetTournamentCategoryLeaderBoard(TournamentId tournamentId,
                                                                                        CategoryId categoryId)
        {
            return _dbContext.LeaderboardTournamentCategoryItems
                .Where(x => x.TournamentId == tournamentId && x.CategoryId == categoryId)
                .ToList();
        }

        public void UpdateCompetitionLeaderBoardItem(LeaderboardCompetitionCategoryItem item)
        {
            _dbContext.LeaderboardCompetitionCategoryItems.Update(item);
        }

        public void UpdateTournamentLeaderBoardItem(LeaderboardTournamentCategoryItem item)
        {
            _dbContext.LeaderboardTournamentCategoryItems.Update(item);
        }

        public void RemoveFisherFromLeaderboardCategory(TournamentId tournamentId,
                                                        IEnumerable<CompetitionId> tournamentCompetitions,
                                                        CategoryId categoryId,
                                                        FisherId fisherId)
        {
            _dbContext.LeaderboardCompetitionCategoryItems
                .Where(x => tournamentCompetitions.Contains(x.CompetitionId) && x.CategoryId == categoryId && x.FisherId == fisherId)
                .ExecuteDelete();

            _dbContext.LeaderboardTournamentCategoryItems
                .Where(x => x.TournamentId == tournamentId && x.CategoryId == categoryId && x.FisherId == fisherId)
                .ExecuteDelete();
        }
    }
}