using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Domain.UserAggregate;
using FisherTournament.Infrastracture.Mediator;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Infrastracture.Persistence;

public class TournamentFisherDbContext : DbContext, ITournamentFisherDbContext
{
    public DbSet<Tournament> Tournaments { get; set; } = null!;
    public DbSet<Competition> Competitions { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Fisher> Fishers { get; set; } = null!;

    private readonly IMediator _mediator = null!;

    public TournamentFisherDbContext(
        DbContextOptions<TournamentFisherDbContext> options,
        IMediator mediator)
        : base(options)
    {
        _mediator = mediator;
    }

    public TournamentFisherDbContext(IMediator mediator)
    {
        _mediator = mediator;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TournamentFisherDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _mediator.DispatchDomainEventsBeforeSaveAsync(this, cancellationToken);

        int changes = await base.SaveChangesAsync(cancellationToken);

        await _mediator.DispatchDomainEventsAfterSaveAsync(this, cancellationToken);

        return changes;
    }
}