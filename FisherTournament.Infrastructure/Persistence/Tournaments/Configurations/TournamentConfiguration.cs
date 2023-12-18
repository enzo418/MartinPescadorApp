using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Infrastructure.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FisherTournament.Infrastructure.Persistence.Tournaments.Configurations;

public class TournamentConfigurations : IEntityTypeConfiguration<Tournament>
{
    public void Configure(EntityTypeBuilder<Tournament> builder)
    {
        ConfigureTournament(builder);

        ConfigureTournament_Inscriptions(builder);

        ConfigureTournament_Competitions(builder);

        ConfigureTournament_Category(builder);
    }

    private static void ConfigureTournament_Category(EntityTypeBuilder<Tournament> builder)
    {
        builder.OwnsMany(p => p.Categories, c =>
        {
            c.WithOwner().HasForeignKey("TournamentId");

            c.HasKey(c => c.Id);

            c.Property(c => c.Id)
                .ValueGeneratedOnAdd()
                .HasIntIdConversion();

            c.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(50);
        }).UsePropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureTournament(EntityTypeBuilder<Tournament> builder)
    {
        builder.ToTable("Tournaments");
        builder.HasGuidIdKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired();
        builder.Property(t => t.StartDate)
            .HasUTCValueConversion()
            .IsRequired();
        builder.Property(t => t.EndDate)
            .HasUTCValueConversion()
            .IsRequired(false);
    }

    private static void ConfigureTournament_Inscriptions(EntityTypeBuilder<Tournament> builder)
    {
        builder.OwnsMany(t => t.Inscriptions, i =>
        {
            i.WithOwner()
                .HasForeignKey(i => i.TournamentId);

            // (SQLite does not support generated values on composite keys)
            i.HasKey(/*i => new { i.Id, i.TournamentId }*/ i => i.Id);

            i.Property<int>(i => i.Id)
                .ValueGeneratedOnAdd();

            i.Property(i => i.CategoryId)
                .HasIntIdConversion();

            i.Property(x => x.FisherId)
                    .HasGuidIdConversion();

            i.Property(x => x.TournamentId)
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