using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.UserAggregate;
using FisherTournament.Infrastracture.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FisherTournament.Infrastracture.Persistence.Tournaments.Configurations;

public class FisherConfiguration : IEntityTypeConfiguration<Fisher>
{
    public void Configure(EntityTypeBuilder<Fisher> builder)
    {
        builder.ToTable("Fishers");
        builder.HasGuidIdKey(f => f.Id);

        // Configure 0..1:1 relationship between Fisher and User
        builder.HasOne<User>()
            .WithOne()
            .HasForeignKey<Fisher>(f => f.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.UserId)
                    .HasGuidIdConversion();
    }
}