using FisherTournament.Application.Tournaments.Commands.CloneTournament;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.IntegrationTests;
using FisherTournament.IntegrationTests.Common;
using FisherTournament.ReadModels.Models;
using FisherTournament.ReadModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FisherTournament.UnitTests.Tournaments.Commands
{
	public class CloneTournamentCommandHandlerTest : BaseUseCaseTest
	{
		protected readonly Mock<ILeaderBoardRepository> _leaderBoardRepositoryMock = new();

		public CloneTournamentCommandHandlerTest(UseCaseTestsFixture fixture) : base(fixture)
		{ }

		[Fact]
		public async Task Handler_ShouldNot_CloneTournament_WhenTournamentDoesNotExist()
		{
			// Arrange
			var id = TournamentId.Create(Guid.NewGuid()).Value;
			var command = new CloneTournamentCommand(id.ToString(),
													"New Tournament",
													_fixture.DateTimeProvider.Now);

			// Act
			var result = await _fixture.SendAsync(command);

			// Assert
			result.IsError.Should().BeTrue();
			result.FirstError.Should().Be(Errors.Tournaments.NotFound);
		}

		[Fact]
		public async Task Handler_Should_CloneTournament_WhenTournamentExists()
		{
			using var context = _fixture.TournamentContext;

			// Arrange
			var fisher1 = context.PrepareAdd(Fisher.Create("First1", "Last1"));
			var fisher2 = context.PrepareAdd(Fisher.Create("First2", "Last2"));
			var fisher3 = context.PrepareAdd(Fisher.Create("First3", "Last3"));

			var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
				.WithName("Tournament 1")
				.WithStartDate(_fixture.DateTimeProvider.Now.AddDays(1))
				.WithEndDate(_fixture.DateTimeProvider.Now.AddDays(2))

				.WithCategory("foo")
				.WithCategory("bar")

				.WithInscription(fisher1.Id, 1, "foo")
				.WithInscription(fisher2.Id, 2, "foo")
				.WithInscription(fisher3.Id, 3, "bar")

				.Build(CancellationToken.None);

			var generalCategory = tournament.Categories.Where(c => c.Name == Tournament.GeneralCategoryName).FirstOrDefault();

			_fixture.LeaderboardRepositoryMock = new();
			_fixture.LeaderboardRepositoryMock.Setup(c => c.GetTournamentLeaderBoard(It.IsAny<TournamentId>()))
			.Returns(new List<LeaderboardTournamentCategoryItem>()
			{
				new LeaderboardTournamentCategoryItem()
				{
					FisherId = fisher1.Id,
					CategoryId = generalCategory!.Id,
					Position = 3,
					Positions = new List<int>() { 3 } // prev inscription N° was 1
				},
				new LeaderboardTournamentCategoryItem()
				{
					FisherId = fisher2.Id,
					CategoryId = generalCategory!.Id,
					Position = 1,
					Positions = new List<int>() { 1 } // prev inscription N° was 2
				},
				new LeaderboardTournamentCategoryItem()
				{
					FisherId = fisher3.Id,
					CategoryId = generalCategory!.Id,
					Position = 2,
					Positions = new List<int>() { 2 } // prev inscription N° was 3
				}
			});

			var startDate = _fixture.DateTimeProvider.Now.AddDays(3);
			var command = new CloneTournamentCommand(tournament.Id.ToString(),
													"New Tournament",
													startDate);

			// Act
			var result = await _fixture.SendAsync(command);


			// Assert
			result.IsError.Should().BeFalse();
			result.Value.Name.Should().Be("New Tournament");
			result.Value.StartDate.Should().Be(startDate);
			result.Value.EndDate.Should().BeNull();
			result.Value.Warnings.Should().BeNull();

			var newTournament = await context.Tournaments
										.Where(t => t.Id == TournamentId.Create(result.Value.TournamentId).Value)
										.FirstOrDefaultAsync();

			var newCategoryFoo = newTournament!.Categories.Where(c => c.Name == "foo").FirstOrDefault();
			newCategoryFoo.Should().NotBeNull();

			var newCategoryBar = newTournament!.Categories.Where(c => c.Name == "bar").FirstOrDefault();
			newCategoryBar.Should().NotBeNull();

			newTournament.Should().NotBeNull();

			// Check that the new tournament has the same inscriptions as the old one, but with the new numbers (positions)
			newTournament!.Inscriptions.Count.Should().Be(3);

			var fisher1Insc = newTournament!.Inscriptions.Where(i => i.FisherId == fisher1.Id).First();
			fisher1Insc.CategoryId.Should().Be(newCategoryFoo!.Id);
			fisher1Insc.Number.Should().Be(3);

			var fisher2Insc = newTournament!.Inscriptions.Where(i => i.FisherId == fisher2.Id).First();
			fisher2Insc.CategoryId.Should().Be(newCategoryFoo!.Id);
			fisher2Insc.Number.Should().Be(1);

			var fisher3Insc = newTournament!.Inscriptions.Where(i => i.FisherId == fisher3.Id).First();
			fisher3Insc.CategoryId.Should().Be(newCategoryBar!.Id);
			fisher3Insc.Number.Should().Be(2);
		}
	}
}
