using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.ReadModels.Models;
using FisherTournament.ReadModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Infrastracture.Persistence.ReadModels.EntityFramework.Repositories
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

        public List<TournamentCategoryLbCalculatedItem> CalculateTournamentCategoryLeaderBoard(TournamentId tournamentId, CategoryId categoryId, List<CompetitionId> tournamentCompetitionsId)
        {
            return _dbContext.LeaderboardCompetitionCategoryItems
                 .Where(x => tournamentCompetitionsId.Contains(x.CompetitionId) && x.CategoryId == categoryId)
                 .GroupBy(x => x.FisherId)
                 .OrderBy(x => x.Sum(y => y.Position))
                 .Select(x => new TournamentCategoryLbCalculatedItem(x.Key, x.Sum(y => y.Position),
                                                                     x.Average(y => y.Position), x.Sum(y => y.Score)))
                 .ToList();
        }

        public List<LeaderboardCompetitionCategoryItem> GetCompetitionCategoryLeaderBoard(CompetitionId competitionId, CategoryId categoryId)
        {
            return _dbContext.LeaderboardCompetitionCategoryItems
                .Where(x => x.CompetitionId == competitionId && x.CategoryId == categoryId)
                .ToList();
        }

        public List<LeaderboardTournamentCategoryItem> GetTournamentCategoryLeaderBoard(TournamentId tournamentId, CategoryId categoryId)
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
    }
}