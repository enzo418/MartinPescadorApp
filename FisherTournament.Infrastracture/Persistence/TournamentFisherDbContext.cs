using FisherTournament.Application.Common.Persistence;
using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Domain.UserAggregate;
using Microsoft.EntityFrameworkCore;

namespace FisherTournament.Infrastracture.Persistence;

public class TournamentFisherDbContext : DbContext, ITournamentFisherDbContext
{
    public DbSet<Tournament> Tournaments { get; set; } = null!;
    public DbSet<Competition> Competitions { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Fisher> Fishers { get; set; } = null!;
    public DbSet<CompetitionParticipation> CompetitionParticipations { get; set; } = null!;

    public TournamentFisherDbContext(
        DbContextOptions<TournamentFisherDbContext> options)
        : base(options)
    {

    }

    public TournamentFisherDbContext()
    {

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
}