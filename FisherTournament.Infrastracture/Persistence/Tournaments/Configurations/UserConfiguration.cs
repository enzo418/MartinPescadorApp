using FisherTournament.Domain.UserAggregate;
using FisherTournament.Infrastracture.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FisherTournament.Infrastracture.Persistence.Tournaments.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasGuidIdKey(u => u.Id);
        builder.Property(u => u.FirstName)
            .IsRequired();
        builder.Property(u => u.LastName)
            .IsRequired();
    }
}