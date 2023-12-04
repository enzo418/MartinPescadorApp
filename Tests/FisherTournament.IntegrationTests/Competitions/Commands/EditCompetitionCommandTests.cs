using FisherTournament.Application.Common.Resources;
using FisherTournament.Application.Competitions.Commands.EditCompetition;
using FisherTournament.IntegrationTests.Common;

namespace FisherTournament.IntegrationTests.Competitions.Commands
{
	public class EditCompetitionCommandTests : BaseUseCaseTest
	{
		public EditCompetitionCommandTests(UseCaseTestsFixture fixture) : base(fixture)
		{ }

		[Fact]
		public async Task Handler_Should_EditCompetition()
		{
			using var context = _fixture.TournamentContext;

			var tournament = context.PrepareAdd(Tournament.Create(
				"Test Tournament",
				_fixture.DateTimeProvider.Now.AddDays(1),
				_fixture.DateTimeProvider.Now.AddDays(2))
			);

			await context.SaveChangesAndClear();

			var newCompetition = await CompetitionBuilder.Create(context, _fixture.DateTimeProvider)
				.WithTournament(tournament.Id)
				.WithStartDate(_fixture.DateTimeProvider.Now.AddDays(1))
				.Build(CancellationToken.None);

			var command = new EditCompetitionCommand(
				newCompetition.Id.ToString(),
				_fixture.DateTimeProvider.Now.AddDays(2),
				new CompetitionLocationResource { City = "New City", State = "New State", Country = "New Country", Place = "New Place" },
				true);

			var result = await _fixture.SendAsync(command);
			var modifiedCompetition = await context.FindAsync<Competition>(newCompetition.Id);

			result.IsError.Should().BeFalse($"because the command is valid ({result.Errors.First().Description})");
			modifiedCompetition.Should().NotBeNull();
			modifiedCompetition!.StartDateTime.Should().Be(command.StartDateTime);
			modifiedCompetition.Location.City.Should().Be(command.Location.Value.City);
			modifiedCompetition.Location.State.Should().Be(command.Location.Value.State);
			modifiedCompetition.Location.Country.Should().Be(command.Location.Value.Country);
			modifiedCompetition.Location.Place.Should().Be(command.Location.Value.Place);
			modifiedCompetition.EndDateTime.Should().NotBeNull();
		}

		[Fact]
		public async Task Handler_Should_ReturnError_WhenStartDateBeforeTournament()
		{
			using var context = _fixture.TournamentContext;

			var tournament = context.PrepareAdd(Tournament.Create(
				"Test Tournament",
				_fixture.DateTimeProvider.Now.AddDays(4),
				_fixture.DateTimeProvider.Now.AddDays(6))
			);

			await context.SaveChangesAndClear();

			var newCompetition = await CompetitionBuilder.Create(context, _fixture.DateTimeProvider)
				.WithTournament(tournament.Id)
				.WithStartDate(_fixture.DateTimeProvider.Now.AddDays(6))
				.Build(CancellationToken.None);

			var command = new EditCompetitionCommand(
				newCompetition.Id.ToString(),
				_fixture.DateTimeProvider.Now.AddDays(1),
				null,
				null);

			var result = await _fixture.SendAsync(command);

			result.IsError.Should().BeTrue();
			result.Errors.First().Should().Be(Errors.Competitions.StartDateBeforeTournament);
		}
	}
}
