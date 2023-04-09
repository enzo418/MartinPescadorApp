using App.Metrics;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Domain.UserAggregate;
using FisherTournament.Infrastracture.Mediator;
using FisherTournament.Infrastracture.Persistence.Tournaments.Configurations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Infrastracture.Persistence.Tournaments;

using Application.Common.Metrics;

public class TournamentFisherDbContext : DbContext, ITournamentFisherDbContext
{
    public DbSet<Tournament> Tournaments { get; set; } = null!;
    public DbSet<Competition> Competitions { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Fisher> Fishers { get; set; } = null!;

    private readonly IMediator _mediator = null!;
    private readonly IMetrics _metrics = null!;

    public TournamentFisherDbContext(
        DbContextOptions<TournamentFisherDbContext> options,
        IMediator mediator,
        IMetrics metrics)
        : base(options)
    {
        _mediator = mediator;
        _metrics = metrics;
    }

    public TournamentFisherDbContext(IMediator mediator, IMetrics metrics)
    {
        _mediator = mediator;
        _metrics = metrics;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(TournamentFisherDbContext).Assembly);

        new UserConfiguration().Configure(modelBuilder.Entity<User>());
        new FisherConfiguration().Configure(modelBuilder.Entity<Fisher>());
        new CompetitionConfiguration().Configure(modelBuilder.Entity<Competition>());
        new TournamentConfigurations().Configure(modelBuilder.Entity<Tournament>());

        base.OnModelCreating(modelBuilder);
    }

    // TODO: public override int SaveChanges()

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        using (var timer = _metrics.Measure.Timer.Time(ApplicationMetrics.DatabaseMetrics.SaveChangesBeforeDispatchEventsTimer))
        {
            await _mediator.DispatchDomainEventsBeforeSaveAsync(this, cancellationToken);
        }

        int changes;
        using (var timer = _metrics.Measure.Timer.Time(ApplicationMetrics.DatabaseMetrics.SaveChangesTournamentDbTimer))
        {
            changes = await base.SaveChangesAsync(cancellationToken);
        }

        using (var timer = _metrics.Measure.Timer.Time(ApplicationMetrics.DatabaseMetrics.SaveChangesAfterDispatchEventsTimer))
        {
            await _mediator.DispatchDomainEventsAfterSaveAsync(this, cancellationToken);
        }

        return changes;
    }
}