using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.ReadModels.SeedWork;

namespace FisherTournament.ReadModels.Models;

public class LeaderboardCompetitionCategoryItem : EntityModel
{
    public CompetitionId CompetitionId { get; set; } = null!;
    public FisherId FisherId { get; set; } = null!;
    public int Position { get; set; }
    public CategoryId CategoryId { get; set; } = null!;
    public int Score { get; set; }
    public string? TieBreakingReason { get; set; }
}
