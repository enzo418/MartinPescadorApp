using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FisherTournament.Infrastracture.Persistence.Configurations;

public class CompetitionConfiguration : IEntityTypeConfiguration<Competition>
{
    public void Configure(EntityTypeBuilder<Competition> builder)
    {
        ConfigureCompetition(builder);

        ConfigureCompetition_Participations(builder);

        ConfigureCompetition_Location(builder);
    }

    private static void ConfigureCompetition_Location(EntityTypeBuilder<Competition> builder)
    {
        builder.OwnsOne(c => c.Location);
    }

    private static void ConfigureCompetition(EntityTypeBuilder<Competition> builder)
    {
        builder.ToTable("Competitions");
        builder.HasGuidIdKey(c => c.Id);

        builder.Property(c => c.StartDateTime).IsRequired();
        builder.Property(c => c.EndDateTime).IsRequired();

        builder.HasOne<Tournament>()
            .WithMany()
            .HasForeignKey(c => c.TournamentId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(c => c.TournamentId)
            .HasGuidIdConversion();
    }

    private static void ConfigureCompetition_Participations(EntityTypeBuilder<Competition> builder)
    {
        builder.OwnsMany(c => c.Participations, p =>
        {
            p.ToTable("CompetitionParticipations");
            p.WithOwner().HasForeignKey(p => p.CompetitionId);
            p.HasKey(p => new { p.Id, p.CompetitionId });
            p.Property<int>(p => p.Id)
                .ValueGeneratedOnAdd();

            p.Property(x => x.FisherId)
                    .HasGuidIdConversion();

            p.HasOne<Fisher>()
                .WithMany()
                .HasForeignKey(p => p.FisherId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // p.Ignore(x => x.FishCaught);
            p.OwnsMany(p => p.FishCaught, f =>
            {
                f.ToTable("CompetitionParticipationFishCaught");
                f.WithOwner()
                    .HasForeignKey("CompetitionParticipationId", "CompetitionId");

                f.HasKey("Id", "CompetitionParticipationId", "CompetitionId");

                f.Property<int>(x => x.Id)
                    .ValueGeneratedOnAdd();

                f.Property<int>(x => x.Score)
                    .IsRequired();

                f.Property(x => x.FisherId)
                    .HasGuidIdConversion();

                f.HasOne<Fisher>()
                    .WithMany()
                    .HasForeignKey(x => x.FisherId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.NoAction); // multiple cascade paths.
            }).UsePropertyAccessMode(PropertyAccessMode.Field);
        }).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}