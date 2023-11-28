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

        builder.Property(u => u.DNI)
            .IsRequired();

        // Configure 0..1:1 relationship between Fisher and User
        //builder.HasOne<Fisher>()
        //    .WithOne()
        //    .HasForeignKey<User>(p => p.FisherId)
        //    .OnDelete(DeleteBehavior.NoAction);

        builder.Property(x => x.FisherId)
                    .HasNullableGuidIdConversion();
    }
}