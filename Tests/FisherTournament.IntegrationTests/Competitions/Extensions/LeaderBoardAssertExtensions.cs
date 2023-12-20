using FisherTournament.Application.Competitions.Queries.GetLeaderBoard;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.IntegrationTests.Competitions.Extensions
{
    // Fluent Assertion for the leaderboard tests, this makes it easier to write/read but difficult to pinpoint the exact error
    // just form the console without debugging.
    internal static class LeaderBoardAssertExtensions
    {
        public static LeaderBoardCategory ShouldHaveCategoryLeaderBoard(this IEnumerable<LeaderBoardCategory> leaderBoardCategories, string Name, CategoryId Id)
        {
            var category = leaderBoardCategories.Single(c => c.Id == Id);
            category.Name.Should().Be(Name);
            return category;
        }

        public static LeaderBoardCategory ShouldHaveCategoryLeaderBoard(this IEnumerable<LeaderBoardCategory> leaderBoardCategories, string Name, string Id)
        {
            var category = leaderBoardCategories.Single(c => c.Id == Id);
            category.Name.Should().Be(Name);
            return category;
        }

        public static IEnumerable<LeaderBoardItem> ShouldHaveNPositions(this LeaderBoardCategory leaderBoardCategory, int n)
        {
            leaderBoardCategory.LeaderBoard.Should().HaveCount(n);
            return leaderBoardCategory.LeaderBoard;
        }

        public static IEnumerable<LeaderBoardItem> ShouldHavePosition(
            this IEnumerable<LeaderBoardItem> leaderBoard,
            int position,
            FisherId fisherId,
            int totalScore,
            string? tieBreakingReason = null)
        {
            var leaderBoardPosition = leaderBoard.Where(p => p.Position == position).ToList();

            leaderBoardPosition.Count.Should().BeGreaterThan(0);

            if (leaderBoardPosition.Count == 1)
            {
                leaderBoardPosition.First().FisherId.Should().Be(fisherId.ToString());
                leaderBoardPosition.First().TotalScore.Should().Be(totalScore);
                leaderBoardPosition.First().TieBreakingReason.Should().Be(tieBreakingReason);
            } else
            {
                totalScore.Should().BeLessThanOrEqualTo(0);

                var fisher = leaderBoardPosition.FirstOrDefault(p => p.FisherId == fisherId.ToString());

                fisher.Should().NotBeNull();

                fisher!.FisherId.Should().Be(fisherId.ToString());
                fisher!.TotalScore.Should().Be(totalScore);
                fisher!.TieBreakingReason.Should().Be(tieBreakingReason);
            }

            return leaderBoard;
        }
    }
}
