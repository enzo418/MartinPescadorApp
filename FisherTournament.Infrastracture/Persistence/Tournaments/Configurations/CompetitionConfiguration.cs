using FisherTournament.Domain.CompetitionAggregate;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Infrastracture.Persistence.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FisherTournament.Infrastracture.Persistence.Tournaments.Configurations;

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
		builder.Property(c => c.EndDateTime); // (nullable)

		builder.Property(c => c.N).IsRequired();

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

			// (SQLite does not support generated values on composite keys)
			p.HasKey(/*p => new { p.Id, p.CompetitionId }*/ p => p.Id);

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
				// (1) (SQLite does not support generated values on composite keys)

				f.ToTable("CompetitionParticipationFishCaught");
				f.WithOwner()
					.HasForeignKey("CompetitionParticipationId"/* (1), "CompetitionId"*/);

				f.HasKey("Id"/* (1), "CompetitionParticipationId", "CompetitionId"*/);

				// (1)
				f.Property(x => x.CompetitionId)
					.HasGuidIdConversion();
				// --

				f.Property<int>(x => x.Id)
					.ValueGeneratedOnAdd();

				f.Property<int>(x => x.Score)
					.IsRequired();

				f.Property(x => x.FisherId)
					.HasGuidIdConversion();

				f.Property(x => x.DateTime)
					.IsRequired();

				f.HasOne<Fisher>()
					.WithMany()
					.HasForeignKey(x => x.FisherId)
					.IsRequired()
					.OnDelete(DeleteBehavior.NoAction); // multiple cascade paths.
			}).UsePropertyAccessMode(PropertyAccessMode.Field);
		}).UsePropertyAccessMode(PropertyAccessMode.Field);
	}
}