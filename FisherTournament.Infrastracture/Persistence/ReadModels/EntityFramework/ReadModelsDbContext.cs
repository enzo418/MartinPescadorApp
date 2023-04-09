using App.Metrics;
using FisherTournament.Infrastracture.Persistence.ReadModels.EntityFramework.Configurations;
using FisherTournament.ReadModels.Models;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Infrastracture.Persistence.ReadModels.EntityFramework;

using Application.Common.Metrics;


public class ReadModelsDbContext : DbContext
{
    public DbSet<LeaderboardCompetitionCategoryItem> LeaderboardCompetitionCategoryItems { get; set; } = null!;
    public DbSet<LeaderboardTournamentCategoryItem> LeaderboardTournamentCategoryItems { get; set; } = null!;

    private readonly IMetrics _metrics = null!;

    public ReadModelsDbContext(DbContextOptions<ReadModelsDbContext> options, IMetrics metrics)
        : base(options)
    {
        _metrics = metrics;
    }

    public ReadModelsDbContext(IMetrics metrics)
    {
        _metrics = metrics;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var leaderBoardConfigurator = new LeaderBoardReadModelsConfiguration();
        leaderBoardConfigurator.Configure(modelBuilder.Entity<LeaderboardCompetitionCategoryItem>());
        leaderBoardConfigurator.Configure(modelBuilder.Entity<LeaderboardTournamentCategoryItem>());

        // configure 
        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        using var _ = _metrics.Measure.Timer.Time(ApplicationMetrics.DatabaseMetrics.SaveChangesReadModelsDbTimer);

        int res = base.SaveChanges();

        return res;
    }

    public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        using var _ = _metrics.Measure.Timer.Time(ApplicationMetrics.DatabaseMetrics.SaveChangesReadModelsDbTimer);
        int res = await base.SaveChangesAsync(cancellationToken);

        return res;
    }
}