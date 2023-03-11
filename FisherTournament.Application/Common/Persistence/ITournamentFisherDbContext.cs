using Microsoft.EntityFrameworkCore;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.UserAggregate;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.CompetitionAggregate.Entities;

namespace FisherTournament.Application.Common.Persistence;

public interface ITournamentFisherDbContext
{
    DbSet<Tournament> Tournaments { get; }
    DbSet<Competition> Competitions { get; }
    DbSet<User> Users { get; }
    DbSet<Fisher> Fishers { get; }

    DbSet<CompetitionParticipation> CompetitionParticipations { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}