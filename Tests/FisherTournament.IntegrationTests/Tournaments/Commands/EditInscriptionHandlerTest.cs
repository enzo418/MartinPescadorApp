using FisherTournament.Application.Tournaments.Commands.EditInscription;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.IntegrationTests.Common;

namespace FisherTournament.IntegrationTests.Tournaments.Commands
{
	public class EditInscriptionHandlerTest : BaseUseCaseTest
	{
		public EditInscriptionHandlerTest(UseCaseTestsFixture fixture) : base(fixture)
		{ }

		[Fact]
		public async Task Handler_Should_EditInscription()
		{
			using var context = _fixture.TournamentContext;

			var fisher = context.PrepareFisher("First", "Last", out var _);

			var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
				.WithName("Test Tournament")
				.WithStartDate(_fixture.DateTimeProvider.Now.AddDays(1))
				.WithEndDate(_fixture.DateTimeProvider.Now.AddDays(2))
				.WithCategory("foo")
				.WithCategory("bar")
				.WithInscription(fisher.Id, 1, "foo")
				.Build(CancellationToken.None);

			var category2 = tournament.Categories.Where(c => c.Name == "bar").FirstOrDefault();

			var command = new EditInscriptionCommand(tournament.Id.ToString(),
													fisher.Id.ToString(),
													category2.Id,
													2);

			// Act
			var result = await _fixture.SendAsync(command);
			var tournamentWithEditedInscription = await context.FindAsync<Tournament>(tournament.Id);

			// Assert
			result.IsError.Should().BeFalse($"because the command is valid ({result.Errors.First().Description})");
			tournamentWithEditedInscription.Should().NotBeNull();
			tournamentWithEditedInscription!.Inscriptions.Should()
				.HaveCount(1);
			tournamentWithEditedInscription!.Inscriptions.First()
				.Number.Should().Be(2);
			tournamentWithEditedInscription!.Inscriptions.First()
				.CategoryId.Should().Be(category2.Id);
		}


		[Fact]
		public async Task Handler_Should_ReturnError_When_TournamentIsOver()
		{
			using var context = _fixture.TournamentContext;

			var fisher = context.PrepareFisher("First", "Last", out var _);

			var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
				.WithName("Test Tournament")
				.WithStartDate(_fixture.DateTimeProvider.Now.AddDays(-2))
				.WithEndDate(_fixture.DateTimeProvider.Now.AddDays(-1))
				.WithCategory("test")
				.Build(CancellationToken.None);

			var category = tournament.Categories.Where(c => c.Name == "test").FirstOrDefault();

			tournament.AddInscription(fisher.Id, category.Id, 1, _fixture.DateTimeProvider);

			await context.SaveChangesAndClear();

			var command = new EditInscriptionCommand(tournament.Id.ToString(),
													fisher.Id.ToString(),
													category.Id,
													2);

			// Act
			var result = await _fixture.SendAsync(command);

			// Assert
			result.IsError.Should().BeTrue();
			result.Errors.First().Should().Be(Errors.Tournaments.IsOver);
		}

		[Fact]
		public async Task Handler_Should_ReturnError_When_InscriptionNotFound()
		{
			using var context = _fixture.TournamentContext;

			var fisher = context.PrepareFisher("asd", "aaaaa", out var _);

			var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
				.WithName("Test Tournament")
				.WithCategory("test")
				.WithStartDate(_fixture.DateTimeProvider.Now.AddDays(1))
				.WithEndDate(_fixture.DateTimeProvider.Now.AddDays(2))
				.Build(CancellationToken.None);

			var category = tournament.Categories.Where(c => c.Name == "test").FirstOrDefault();

			var command = new EditInscriptionCommand(tournament.Id.ToString(),
													fisher.Id.ToString(),
													category.Id,
													2);

			// Act
			var result = await _fixture.SendAsync(command);

			// Assert
			result.IsError.Should().BeTrue();
			result.Errors.First().Should().Be(Errors.Tournaments.InscriptionNotFound);
		}

		[Fact]
		public async Task Handler_Should_ReturnError_When_CategoryNotFound()
		{
			using var context = _fixture.TournamentContext;

			var fisher = context.PrepareFisher("asd", "aaa", out var _);

			var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
				.WithName("Test Tournament")
				.WithStartDate(_fixture.DateTimeProvider.Now.AddDays(1))
				.WithEndDate(_fixture.DateTimeProvider.Now.AddDays(2))
				.WithCategory("foo")
				.WithInscription(fisher.Id, 1, "foo")
				.Build(CancellationToken.None);

			var command = new EditInscriptionCommand(tournament.Id.ToString(),
													fisher.Id.ToString(),
													CategoryId.Create(9000).Value,
													2);

			// Act
			var result = await _fixture.SendAsync(command);

			// Assert
			result.IsError.Should().BeTrue();
			result.Errors.First().Should().Be(Errors.Categories.NotFound);
		}

		[Fact]
		public async Task Handler_Should_ReturnError_When_InscriptionNumberAlreadyExists()
		{
			using var context = _fixture.TournamentContext;

			var fisher1 = context.PrepareFisher("First", "Last", out var _);
			var fisher2 = context.PrepareFisher("Second", "Last", out var _);

			var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
				.WithName("Test Tournament")
				.WithStartDate(_fixture.DateTimeProvider.Now.AddDays(1))
				.WithEndDate(_fixture.DateTimeProvider.Now.AddDays(2))
				.WithCategory("test")
				.WithInscription(fisher1.Id, 1, "test")
				.WithInscription(fisher2.Id, 2, "test")
				.Build(CancellationToken.None);

			var category = tournament.Categories.Where(c => c.Name == "test").FirstOrDefault();

			var command = new EditInscriptionCommand(tournament.Id.ToString(),
													fisher1.Id.ToString(),
													category.Id,
													2);

			// Act
			var result = await _fixture.SendAsync(command);

			// Assert
			result.IsError.Should().BeTrue();
			result.Errors.First().Should().Be(Errors.Tournaments.InscriptionNumberAlreadyExists);
		}

		[Fact]
		public async Task Handler_Should_ReturnError_When_FisherHasAlreadyScored()
		{
			using var context = _fixture.TournamentContext;

			var fisher = context.PrepareFisher("First", "Last", out var _);

			var tournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
				.WithName("Test Tournament")
				.WithStartDate(_fixture.DateTimeProvider.Now.AddDays(1))
				.WithEndDate(_fixture.DateTimeProvider.Now.AddDays(2))
				.WithCategory("test")
				.WithInscription(fisher.Id, 1, "test")
				.Build(CancellationToken.None);

			var category = tournament.Categories.Where(c => c.Name == "test").FirstOrDefault();

			var competition = await CompetitionBuilder.Create(context, _fixture.DateTimeProvider)
				.WithTournament(tournament.Id)
				.WithScore(fisher.Id, 20)
				.Build(CancellationToken.None);

			var command = new EditInscriptionCommand(
				tournament.Id.ToString(),
				FisherId: fisher.Id.ToString(),
				CategoryId: null,
				NewNumber: null);

			// Act
			var result = await _fixture.SendAsync(command);

			// Assert
			result.IsError.Should().BeTrue();
			result.Errors.First().Should().Be(Errors.Tournaments.FisherHasAlreadyScored);
		}
	}
}
