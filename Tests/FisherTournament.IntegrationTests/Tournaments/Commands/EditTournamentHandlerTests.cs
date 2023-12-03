using FisherTournament.Application.Tournaments.Commands.EditTournament;
using FisherTournament.IntegrationTests.Common;

namespace FisherTournament.IntegrationTests.Tournaments.Commands
{
    public class EditTournamentHandlerTests : BaseUseCaseTest
    {
        public EditTournamentHandlerTests(UseCaseTestsFixture fixture) : base(fixture)
        { }


        [Fact]
        public async Task Handler_Should_EditTournament()
        {
            // 
            using var context = _fixture.TournamentContext;

            var newTournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
                .WithName("Test Tournament")
                .WithStartDate(_fixture.DateTimeProvider.Now.AddDays(1))
                .WithEndDate(_fixture.DateTimeProvider.Now.AddDays(2))
                .Build(CancellationToken.None);


            var command = new EditTournamentCommand(
                newTournament.Id.ToString(),
                "Omega Tournament",
                _fixture.DateTimeProvider.Now.AddDays(1),
                TournamentFinishedState: false);

            // 
            var result = await _fixture.SendAsync(command);
            var modifiedTournament = await context.FindAsync<Tournament>(newTournament.Id);

            // 
            result.IsError.Should().BeFalse($"because the command is valid ({result.Errors.First().Description})");
            modifiedTournament.Should().NotBeNull();
            modifiedTournament!.Name.Should().Be("Omega Tournament");
            modifiedTournament.StartDate.Should().Be(command.StartDate);
            modifiedTournament.EndDate.Should().BeNull();
        }

        [Fact]
        public async Task Handler_Should_EditTournamentWithCompetition()
        {
            using var context = _fixture.TournamentContext;

            var newTournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
                .WithName("Test Tournament")
                .WithStartDate(_fixture.DateTimeProvider.Now.AddDays(1))
                .Build(CancellationToken.None);

            var competition = await CompetitionBuilder.Create(context, _fixture.DateTimeProvider)
                .WithTournament(newTournament.Id)
                .WithStartDate(_fixture.DateTimeProvider.Now.AddDays(2))
                .Build(CancellationToken.None);

            var command = new EditTournamentCommand(
                newTournament.Id.ToString(),
                "Omega Tournament",
                _fixture.DateTimeProvider.Now.AddDays(2).AddHours(-5),
                TournamentFinishedState: null);

            // 
            var result = await _fixture.SendAsync(command);
            var modifiedTournament = await context.FindAsync<Tournament>(newTournament.Id);

            // 
            result.IsError.Should().BeFalse($"because the command is valid ({result.Errors.First().Description})");
            modifiedTournament.Should().NotBeNull();
            modifiedTournament!.Name.Should().Be("Omega Tournament");
            modifiedTournament.StartDate.Should().Be(command.StartDate);
            modifiedTournament.EndDate.Should().BeNull();
        }

        [Fact]
        public async Task Handler_Should_ReturnError_WhenCompetitionStartsEarlierThanNewStartDate()
        {
            // 
            using var context = _fixture.TournamentContext;

            var newTournament = await TournamentBuilder.Create(context, _fixture.DateTimeProvider)
                .WithName("Test Tournament")
                .WithStartDate(_fixture.DateTimeProvider.Now.AddDays(1))
                .WithEndDate(_fixture.DateTimeProvider.Now.AddDays(2))
                .Build(CancellationToken.None);

            var competition = await CompetitionBuilder.Create(context, _fixture.DateTimeProvider)
                .WithTournament(newTournament.Id)
                .WithStartDate(_fixture.DateTimeProvider.Now.AddDays(2))
                .Build(CancellationToken.None);

            var command = new EditTournamentCommand(
                newTournament.Id.ToString(),
                null,
                _fixture.DateTimeProvider.Now.AddDays(3),
                null);

            // 
            var result = await _fixture.SendAsync(command);

            // 
            result.IsError.Should().BeTrue();
            result.Errors.First().Should().Be(Errors.Tournaments.CompetitionHasEarlierStartDate);
        }
    }
}
