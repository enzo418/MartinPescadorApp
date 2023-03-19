using FisherTournament.Application.Tournaments.Commands.AddInscription;

namespace FisherTournament.IntegrationTests.Tournaments.Commands
{
    public class AddInscriptionHandlerTest : BaseUseCaseTest
    {
        public AddInscriptionHandlerTest(UseCaseTestsFixture fixture) : base(fixture)
        { }

        [Fact]
        public async Task Handler_Should_AddInscription()
        {
            // Arrange
            var tournament = await _fixture.AddAsync(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(1),
                _fixture.DateTimeProvider.Now.AddDays(2)));
            var user = await _fixture.AddAsync(User.Create("First", "Last"));
            var fisher = await _fixture.AddAsync(Fisher.Create(user.Id));

            var command = new AddInscriptionCommand(tournament.Id.ToString(), fisher.Id.ToString());

            // Act
            var result = await _fixture.SendAsync(command);
            var tournamentWithInscription = await _fixture.FindAsync<Tournament>(tournament.Id);

            // Assert
            result.IsError.Should().BeFalse($"because the command is valid ({result.Errors.First().Description})");
            tournamentWithInscription.Should().NotBeNull();
            tournamentWithInscription!.Inscriptions.Should()
                .HaveCount(1)
                .And
                .Contain(i => i.FisherId == fisher.Id);
        }

        [Fact]
        public async Task Handler_Should_NotAddInscription_When_TournamentDoesntExist()
        {
            // Arrange
            var user = await _fixture.AddAsync(User.Create("First", "Last"));
            var fisher = await _fixture.AddAsync(Fisher.Create(user.Id));

            var command = new AddInscriptionCommand(Guid.NewGuid().ToString(), fisher.Id.ToString());

            // Act
            var result = await _fixture.SendAsync(command);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournament.NotFound);
        }

        [Fact]
        public async Task Handler_Should_NotAddInscription_When_FisherDoesntExist()
        {
            // Arrange
            var tournament = await _fixture.AddAsync(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(1),
                _fixture.DateTimeProvider.Now.AddDays(2)));

            var command = new AddInscriptionCommand(tournament.Id.ToString(), Guid.NewGuid().ToString());

            // Act
            var result = await _fixture.SendAsync(command);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Fisher.NotFound);
        }

        [Fact]
        public async Task Handler_Should_NotAddInscription_When_FisherIsAlreadyInscribed()
        {
            // Arrange
            var user = await _fixture.AddAsync(User.Create("First", "Last"));
            var fisher = await _fixture.AddAsync(Fisher.Create(user.Id));

            var tournament = await _fixture.AddAsync(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(1),
                _fixture.DateTimeProvider.Now.AddDays(2)),
                beforeSave: t => t.AddInscription(fisher.Id, _fixture.DateTimeProvider));

            var command = new AddInscriptionCommand(tournament.Id.ToString(), fisher.Id.ToString());

            // Act
            var errorResult = await _fixture.SendAsync(command);

            // Assert
            errorResult.IsError.Should().BeTrue();
            errorResult.FirstError.Should().Be(Errors.Tournament.InscriptionAlreadyExists);
        }

        [Fact]
        public async Task Handler_Should_NotAddInscription_When_TournamentIsClosed()
        {
            // Arrange
            var tournament = await _fixture.AddAsync(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(-2),
                _fixture.DateTimeProvider.Now.AddDays(-1)));
            var user = await _fixture.AddAsync(User.Create("First", "Last"));
            var fisher = await _fixture.AddAsync(Fisher.Create(user.Id));

            var command = new AddInscriptionCommand(tournament.Id.ToString(), fisher.Id.ToString());

            // Act
            var result = await _fixture.SendAsync(command);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournament.IsOver);
        }
    }
}