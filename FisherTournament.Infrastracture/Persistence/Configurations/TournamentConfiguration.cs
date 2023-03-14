using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.TournamentAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FisherTournament.Infrastracture.Persistence.Configurations;

public class TournamentConfigurations : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        ConfigureTournament(builder);

        ConfigureTournament_Inscriptions(builder);

        ConfigureTournament_Competitions(builder);
    }

    private static void ConfigureTournament(EntityTypeBuilder<Tournament> builder)
    {
        builder.ToTable("Tournaments");
        builder.HasGuidIdKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.StartDate).IsRequired();
    }

    private static void ConfigureTournament_Inscriptions(EntityTypeBuilder<Tournament> builder)
    {
        builder.OwnsMany(t => t.Inscriptions, i =>
        {
            i.WithOwner().HasForeignKey(i => i.TournamentId);

            // (SQLite does not support generated values on composite keys)
            i.HasKey(/*i => new { i.Id, i.TournamentId }*/ i => i.Id);

            i.Property<int>(i => i.Id)
                .ValueGeneratedOnAdd();

            i.Property(x => x.FisherId)
                    .HasGuidIdConversion();

            i.HasOne<Fisher>()
                .WithMany()
                .HasForeignKey(i => i.FisherId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureTournament_Competitions(EntityTypeBuilder<Tournament> builder)
    {
        builder.OwnsMany(t => t.CompetitionsIds, c =>
        {
            c.ToTable("TournamentCompetitionsIds");

            c.HasKey("Id"); // Shadow id

            c.WithOwner().HasForeignKey("TournamentId");

            c.Property(c => c.Value)
                .HasColumnName("CompetitionId")
                .ValueGeneratedNever();

            //// Doesn't work:
            // c.HasOne(typeof(Competition))
            //     .WithMany()
            //     .HasForeignKey("CompetitionId")
            //     .HasPrincipalKey("Id")
            //     .IsRequired()
            //     .OnDelete(DeleteBehavior.Cascade);
            // c.Property("CompetitionId")
            //     .HasGuidIdConversion<CompetitionId>();
        }).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}