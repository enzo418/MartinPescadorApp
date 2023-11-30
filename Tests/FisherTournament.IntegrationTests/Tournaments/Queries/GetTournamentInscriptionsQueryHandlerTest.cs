using FisherTournament.Application.Common.Requests;
using FisherTournament.Application.Tournaments.Queries.GetTournamentInscriptions;
using FisherTournament.IntegrationTests.Common;

namespace FisherTournament.IntegrationTests.Tournaments.Queries
{
	public static class ExtensionAssert
	{
		public static void ShouldHaveInscription(this PagedList<GetTournamentInscriptionsQueryResult> result,
						int number, string fisherName, string categoryName)
		{
			result.Items.Should().Contain(i =>
							i.Number == number
						   && i.FisherName == fisherName
						   && i.CategoryName == categoryName);
		}
	}

	public class GetTournamentInscriptionsQueryHandlerTest : BaseUseCaseTest
	{
		public GetTournamentInscriptionsQueryHandlerTest(UseCaseTestsFixture fixture) : base(fixture)
		{
		}

		[Fact]
		public async Task Handler_Should_ReturnEmptyList()
		{
			// Arrange
			using var context = _fixture.TournamentContext;

			var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
				.WithName("Test Tournament")
				.WithStartDate(_fixture.DateTimeProvider.Now)
				.WithEndDate(_fixture.DateTimeProvider.Now.AddDays(1))

				.WithCategory("Primary")
				.WithCategory("Secondary")

				.Build(CancellationToken.None);

			// Act
			var result = await _fixture.SendAsync(new GetTournamentInscriptionsQuery(tournament.Id.ToString(), null, 1, 1));

			// Assert
			result.IsError.Should().BeFalse();
			result.Value.Should().NotBeNull();
			result.Value!.Items.Should().BeEmpty();
			result.Value!.TotalCount.Should().Be(0);
		}

		[Fact]
		public async Task Handler_Should_ReturnValidListWithoutAssociatedUser()
		{
			using var context = _fixture.TournamentContext;

			var fisher1 = context.PrepareFisherWithoutAssociatedUser("First1", "Last1");
			var fisher2 = context.PrepareFisherWithoutAssociatedUser("First2", "Last2");
			var fisher3 = context.PrepareFisherWithoutAssociatedUser("First3", "Last3");
			var fisher4 = context.PrepareFisherWithoutAssociatedUser("First4", "Last4");

			var fisher5 = context.PrepareFisherWithoutAssociatedUser("First5", "Last5");
			var fisher6 = context.PrepareFisherWithoutAssociatedUser("First6", "Last6");

			var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
				.WithName("Test Tournament")
				.WithStartDate(_fixture.DateTimeProvider.Now)
				.WithEndDate(_fixture.DateTimeProvider.Now.AddDays(1))

				.WithCategory("Primary")
				.WithCategory("Secondary")

				.WithInscription(fisher1.Id, 1, "Primary")
				.WithInscription(fisher2.Id, 2, "Primary")
				.WithInscription(fisher3.Id, 3, "Primary")
				.WithInscription(fisher4.Id, 4, "Primary")

				.WithInscription(fisher5.Id, 5, "Secondary")
				.WithInscription(fisher6.Id, 6, "Secondary")

				.Build(CancellationToken.None);

			// Act
			var result = await _fixture.SendAsync(new GetTournamentInscriptionsQuery(tournament.Id.ToString(), "", 1, 100));

			// Assert
			result.IsError.Should().BeFalse();
			result.Value.Should().NotBeNull();
			result.Value!.TotalCount.Should().Be(6);
			result.Value!.Items.Should().HaveCount(6);

			result.Value.ShouldHaveInscription(1, Fisher.GetName("First1", "Last1"), "Primary");
			result.Value.ShouldHaveInscription(2, Fisher.GetName("First2", "Last2"), "Primary");
			result.Value.ShouldHaveInscription(3, Fisher.GetName("First3", "Last3"), "Primary");
			result.Value.ShouldHaveInscription(4, Fisher.GetName("First4", "Last4"), "Primary");
			result.Value.ShouldHaveInscription(5, Fisher.GetName("First5", "Last5"), "Secondary");
			result.Value.ShouldHaveInscription(6, Fisher.GetName("First6", "Last6"), "Secondary");
		}

		[Fact]
		public async Task Handler_Should_ReturnValidList()
		{
			using var context = _fixture.TournamentContext;

			var fisher1 = context.PrepareFisher("First1", "Last1", out var _);
			var fisher2 = context.PrepareFisher("First2", "Last2", out var _);
			var fisher3 = context.PrepareFisher("First3", "Last3", out var _);
			var fisher4 = context.PrepareFisher("First4", "Last4", out var _);

			var fisher5 = context.PrepareFisher("First5", "Last5", out var _);
			var fisher6 = context.PrepareFisher("First6", "Last6", out var _);

			var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
				.WithName("Test Tournament")
				.WithStartDate(_fixture.DateTimeProvider.Now)
				.WithEndDate(_fixture.DateTimeProvider.Now.AddDays(1))

				.WithCategory("Primary")
				.WithCategory("Secondary")

				.WithInscription(fisher1.Id, 1, "Primary")
				.WithInscription(fisher2.Id, 2, "Primary")
				.WithInscription(fisher3.Id, 3, "Primary")
				.WithInscription(fisher4.Id, 4, "Primary")

				.WithInscription(fisher5.Id, 5, "Secondary")
				.WithInscription(fisher6.Id, 6, "Secondary")

				.Build(CancellationToken.None);

			// Act
			var result = await _fixture.SendAsync(new GetTournamentInscriptionsQuery(tournament.Id.ToString(), "", 1, 100));

			// Assert
			result.IsError.Should().BeFalse();
			result.Value.Should().NotBeNull();
			result.Value!.TotalCount.Should().Be(6);
			result.Value!.Items.Should().HaveCount(6);

			result.Value.ShouldHaveInscription(1, Fisher.GetName("First1", "Last1"), "Primary");
			result.Value.ShouldHaveInscription(2, Fisher.GetName("First2", "Last2"), "Primary");
			result.Value.ShouldHaveInscription(3, Fisher.GetName("First3", "Last3"), "Primary");
			result.Value.ShouldHaveInscription(4, Fisher.GetName("First4", "Last4"), "Primary");
			result.Value.ShouldHaveInscription(5, Fisher.GetName("First5", "Last5"), "Secondary");
			result.Value.ShouldHaveInscription(6, Fisher.GetName("First6", "Last6"), "Secondary");
		}

		[Fact]
		public async Task Handler_Should_ReturnFilteredList_WhenPassedCategoryName()
		{
			using var context = _fixture.TournamentContext;

			var fisher1 = context.PrepareFisher("First1", "Last1", out var _);
			var fisher2 = context.PrepareFisher("First2", "Last2", out var _);
			var fisher3 = context.PrepareFisher("First3", "Last3", out var _);
			var fisher4 = context.PrepareFisher("First4", "Last4", out var _);

			var fisher5 = context.PrepareFisher("First5", "Last5", out var _);
			var fisher6 = context.PrepareFisher("First6", "Last6", out var _);

			var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
				.WithName("Test Tournament")
				.WithStartDate(_fixture.DateTimeProvider.Now)
				.WithEndDate(_fixture.DateTimeProvider.Now.AddDays(1))

				.WithCategory("Primary")
				.WithCategory("Secondary")

				.WithInscription(fisher1.Id, 1, "Primary")
				.WithInscription(fisher2.Id, 2, "Primary")
				.WithInscription(fisher3.Id, 3, "Primary")
				.WithInscription(fisher4.Id, 4, "Primary")

				.WithInscription(fisher5.Id, 5, "Secondary")
				.WithInscription(fisher6.Id, 6, "Secondary")

				.Build(CancellationToken.None);

			var primaryCategory = tournament.Categories.First(c => c.Name == "Primary");

			// Act
			var result = await _fixture.SendAsync(new GetTournamentInscriptionsQuery(tournament.Id.ToString(), primaryCategory.Id.ToString(), 1, 100));

			// Assert
			result.IsError.Should().BeFalse();
			result.Value.Should().NotBeNull();
			result.Value!.TotalCount.Should().Be(4);
			result.Value!.Items.Should().HaveCount(4);

			result.Value.ShouldHaveInscription(1, Fisher.GetName("First1", "Last1"), "Primary");
			result.Value.ShouldHaveInscription(2, Fisher.GetName("First2", "Last2"), "Primary");
			result.Value.ShouldHaveInscription(3, Fisher.GetName("First3", "Last3"), "Primary");
			result.Value.ShouldHaveInscription(4, Fisher.GetName("First4", "Last4"), "Primary");
		}

		[Fact]
		public async Task Handler_Should_ReturnPagedList_WhenPassedPageNumberAndPageSize()
		{
			using var context = _fixture.TournamentContext;

			var fisher1 = context.PrepareFisher("First1", "Last1", out var _);
			var fisher2 = context.PrepareFisher("First2", "Last2", out var _);
			var fisher3 = context.PrepareFisher("First3", "Last3", out var _);
			var fisher4 = context.PrepareFisher("First4", "Last4", out var _);

			var fisher5 = context.PrepareFisher("First5", "Last5", out var _);
			var fisher6 = context.PrepareFisher("First6", "Last6", out var _);

			var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
				.WithName("Test Tournament")
				.WithStartDate(_fixture.DateTimeProvider.Now)
				.WithEndDate(_fixture.DateTimeProvider.Now.AddDays(1))

				.WithCategory("Primary")
				.WithCategory("Secondary")

				.WithInscription(fisher1.Id, 1, "Primary")
				.WithInscription(fisher2.Id, 2, "Primary")
				.WithInscription(fisher3.Id, 3, "Primary")
				.WithInscription(fisher4.Id, 4, "Primary")

				.WithInscription(fisher5.Id, 5, "Secondary")
				.WithInscription(fisher6.Id, 6, "Secondary")

				.Build(CancellationToken.None);

			var primaryCategory = tournament.Categories.First(c => c.Name == "Primary");
			var secondaryCategory = tournament.Categories.First(c => c.Name == "Secondary");

			// Act
			var result1 = await _fixture.SendAsync(new GetTournamentInscriptionsQuery(tournament.Id.ToString(), primaryCategory.Id.ToString(), Page: 1, PageSize: 2));
			var result2 = await _fixture.SendAsync(new GetTournamentInscriptionsQuery(tournament.Id.ToString(), primaryCategory.Id.ToString(), Page: 2, PageSize: 2));

			var resultSecondary = await _fixture.SendAsync(new GetTournamentInscriptionsQuery(tournament.Id.ToString(), "", Page: 2, PageSize: 4));

			// Assert
			result1.IsError.Should().BeFalse();
			result1.Value.Should().NotBeNull();
			result1.Value!.TotalCount.Should().Be(4);
			result1.Value!.Items.Should().HaveCount(2);

			result1.Value.ShouldHaveInscription(1, Fisher.GetName("First1", "Last1"), "Primary");
			result1.Value.ShouldHaveInscription(2, Fisher.GetName("First2", "Last2"), "Primary");

			result2.IsError.Should().BeFalse();
			result2.Value.Should().NotBeNull();
			result2.Value!.TotalCount.Should().Be(4);
			result2.Value!.Items.Should().HaveCount(2);

			result2.Value.ShouldHaveInscription(3, Fisher.GetName("First3", "Last3"), "Primary");
			result2.Value.ShouldHaveInscription(4, Fisher.GetName("First4", "Last4"), "Primary");

			resultSecondary.IsError.Should().BeFalse();
			resultSecondary.Value.Should().NotBeNull();
			resultSecondary.Value!.TotalCount.Should().Be(6);
			resultSecondary.Value!.Items.Should().HaveCount(2);

			resultSecondary.Value.ShouldHaveInscription(5, Fisher.GetName("First5", "Last5"), "Secondary");
			resultSecondary.Value.ShouldHaveInscription(6, Fisher.GetName("First6", "Last6"), "Secondary");
		}
	}
}