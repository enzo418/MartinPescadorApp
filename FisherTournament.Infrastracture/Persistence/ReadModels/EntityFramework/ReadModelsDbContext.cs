using FisherTournament.Infrastracture.Persistence.ReadModels.EntityFramework.Configurations;
using FisherTournament.ReadModels.Models;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Infrastracture.Persistence.ReadModels.EntityFramework;

using Application.Common.Metrics;


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

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        Tag DbContextTag = new("DbContext", GetType().Name); // Create base DbContext?

        int changes;
        using (var _ = ApplicationMetrics.DatabaseMetrics.SaveChangesHistogram.Time(new Tag("order", "base"), DbContextTag))
        {
            changes = await base.SaveChangesAsync(cancellationToken);
        }

        return changes;
    }

    public override int SaveChanges()
    {
        Tag DbContextTag = new("DbContext", GetType().Name); // Create base DbContext?

        int changes;
        using (var _ = ApplicationMetrics.DatabaseMetrics.SaveChangesHistogram.Time(new Tag("order", "base"), DbContextTag))
        {
            changes = base.SaveChanges();
        }

        return changes;
    }
}