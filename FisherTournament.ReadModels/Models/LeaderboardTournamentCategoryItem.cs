
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.ReadModels.SeedWork;

namespace FisherTournament.ReadModels.Models;

public class LeaderboardTournamentCategoryItem : EntityModel
{
    public TournamentId TournamentId { get; set; } = null!;
    public FisherId FisherId { get; set; } = null!;
    public int Position { get; set; }
    public CategoryId CategoryId { get; set; } = null!;
    public int TotalScore { get; set; }

    /// <summary>
    /// List of positions in each competition, ordered by competition date.
    /// </summary>
    public List<int> Positions { get; set; } = null!;
}