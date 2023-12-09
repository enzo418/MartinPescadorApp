using FisherTournament.Domain.FisherAggregate.ValueObjects;

namespace FisherTournament.Application.LeaderBoard
{
    public class FisherWithScore
    {
        public FisherId FisherId { get; init; } = null!;
        public int Score { get; init; }
        public int LargerPiece { get; init; }

        // a value to use when breaking ties
        public int TieDiscriminator { get; set; } = -1;

        public string? TieBreakingReason { get; set; }
    }
}
