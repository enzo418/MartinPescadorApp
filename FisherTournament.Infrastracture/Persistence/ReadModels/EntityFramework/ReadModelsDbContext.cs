using FisherTournament.Infrastracture.Persistence.ReadModels.EntityFramework.Configurations;
using FisherTournament.ReadModels.Models;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Infrastracture.Persistence.ReadModels.EntityFramework
{
    public class ReadModelsDbContext : DbContext
    {
        public DbSet<LeaderboardCompetitionCategoryItem> LeaderboardCompetitionCategoryItems { get; set; } = null!;
        public DbSet<LeaderboardTournamentCategoryItem> LeaderboardTournamentCategoryItems { get; set; } = null!;

        public ReadModelsDbContext(DbContextOptions<ReadModelsDbContext> options)
            : base(options)
        {
        }

        public ReadModelsDbContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var leaderBoardConfigurator = new LeaderBoardReadModelsConfiguration();
            leaderBoardConfigurator.Configure(modelBuilder.Entity<LeaderboardCompetitionCategoryItem>());
            leaderBoardConfigurator.Configure(modelBuilder.Entity<LeaderboardTournamentCategoryItem>());
            
            // configure 
            base.OnModelCreating(modelBuilder);
        }
    }
}