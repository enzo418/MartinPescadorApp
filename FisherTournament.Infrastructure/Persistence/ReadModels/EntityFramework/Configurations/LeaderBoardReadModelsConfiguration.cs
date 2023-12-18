using FisherTournament.Infrastructure.Persistence.Common;
using FisherTournament.ReadModels.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FisherTournament.Infrastructure.Persistence.ReadModels.EntityFramework.Configurations;

public class LeaderBoardReadModelsConfiguration
    : IEntityTypeConfiguration<LeaderboardCompetitionCategoryItem>,
        IEntityTypeConfiguration<LeaderboardTournamentCategoryItem>
{
    public void Configure(EntityTypeBuilder<LeaderboardCompetitionCategoryItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.CompetitionId).HasGuidIdConversion().IsRequired();
        builder.Property(x => x.CategoryId).HasIntIdConversion().IsRequired();
        builder.Property(x => x.FisherId).HasGuidIdConversion().IsRequired();

        builder.Property(x => x.Position).IsRequired();
        builder.Property(x => x.Score).IsRequired();
    }

    public void Configure(EntityTypeBuilder<LeaderboardTournamentCategoryItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.Property(x => x.TournamentId).HasGuidIdConversion().IsRequired();
        builder.Property(x => x.CategoryId).HasIntIdConversion().IsRequired();
        builder.Property(x => x.FisherId).HasGuidIdConversion().IsRequired();

        builder.Property(x => x.Position).IsRequired();
        builder.Property(x => x.TotalScore).IsRequired();

        builder.Property(x => x.Positions).HasConversion(
            v => string.Join(',', v),
            v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                  .Select(int.Parse)
                  .ToList()
            );
    }
}
