using FisherTournament.Application.Tournaments.Commands.AddInscription;
using FisherTournament.Domain.TournamentAggregate.Entities;

namespace FisherTournament.IntegrationTests.Tournaments.Commands
{
    public class AddInscriptionHandlerTest : BaseUseCaseTest
    {
        public AddInscriptionHandlerTest(UseCaseTestsFixture fixture) : base(fixture)
        { }

        [Fact]
        public async Task Handler_Should_AddInscription()
        {
            using var context = _fixture.Context;

            var tournament = context.PrepareAdd(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(1),
                _fixture.DateTimeProvider.Now.AddDays(2))
            );
            var category = tournament.AddCategory("test");
            var user = context.PrepareAdd(User.Create("First", "Last"));
            var fisher = context.PrepareAdd(Fisher.Create(user.Id));

            // EF NOTE: If you don't clear the change tracker, find will return the same as `tournament` variable,
            // since it was modified in the inscription command (different DB context instance).
            await context.SaveChangesAndClear();

            var command = new AddInscriptionCommand(tournament.Id.ToString(), fisher.Id.ToString(), category.Id);

            // Act
            var result = await _fixture.SendAsync(command);
            var tournamentWithInscription = await context.FindAsync<Tournament>(tournament.Id);

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
            using var context = _fixture.Context;
            var fisher = await context.WithFisherAsync("First", "Last");

            var command = new AddInscriptionCommand(Guid.NewGuid().ToString(), fisher.Id.ToString(), "0");

            // Act
            var result = await _fixture.SendAsync(command);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.NotFound);
        }

        [Fact]
        public async Task Handler_Should_NotAddInscription_When_FisherDoesntExist()
        {
            // Arrange
            using var context = _fixture.Context;
            var tournament = await context.WithAsync(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(1),
                _fixture.DateTimeProvider.Now.AddDays(2)));

            var command = new AddInscriptionCommand(tournament.Id.ToString(), Guid.NewGuid().ToString(), "0");

            // Act
            var result = await _fixture.SendAsync(command);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Fishers.NotFound);
        }

        [Fact]
        public async Task Handler_Should_NotAddInscription_When_FisherIsAlreadyInscribed()
        {
            // Arrange
            using var context = _fixture.Context;
            var user = context.PrepareAdd(User.Create("First", "Last"));
            var fisher = context.PrepareAdd(Fisher.Create(user.Id));

            var tournament = context.PrepareAdd(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(1),
                _fixture.DateTimeProvider.Now.AddDays(2)));

            var category = tournament.AddCategory("test");
            await context.SaveChangesAsync();

            tournament.AddInscription(fisher.Id, category.Id, _fixture.DateTimeProvider);
            await context.SaveChangesAsync();

            // Act
            var errorResult = await _fixture.SendAsync(
                new AddInscriptionCommand(tournament.Id.ToString(), fisher.Id.ToString(), category.Id)
            );

            // Assert
            errorResult.IsError.Should().BeTrue();
            errorResult.FirstError.Should().Be(Errors.Tournaments.InscriptionAlreadyExists);
        }

        [Fact]
        public async Task Handler_Should_NotAddInscription_When_TournamentIsClosed()
        {
            // Arrange
            using var context = _fixture.Context;
            var tournament = context.PrepareAdd(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(-2),
                _fixture.DateTimeProvider.Now.AddDays(-1)));
            var user = context.PrepareAdd(User.Create("First", "Last"));
            var fisher = context.PrepareAdd(Fisher.Create(user.Id));

            var category = tournament.AddCategory("test");
            await context.SaveChangesAsync();

            tournament.AddInscription(fisher.Id, category.Id, _fixture.DateTimeProvider);
            await context.SaveChangesAsync();

            var command = new AddInscriptionCommand(tournament.Id.ToString(), fisher.Id.ToString(), category.Id);

            // Act
            var result = await _fixture.SendAsync(command);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.IsOver);
        }

        [Fact]
        public async Task Handler_Should_NotAddInscription_WhenCategoryDoesntExist()
        {
            // Arrange
            using var context = _fixture.Context;
            var tournament = context.PrepareAdd(Tournament.Create(
                "Test Tournament",
                _fixture.DateTimeProvider.Now.AddDays(1),
                _fixture.DateTimeProvider.Now.AddDays(2)));
            var user = context.PrepareAdd(User.Create("First", "Last"));
            var fisher = context.PrepareAdd(Fisher.Create(user.Id));

            var category = tournament.AddCategory("test");
            await context.SaveChangesAsync();

            var command = new AddInscriptionCommand(tournament.Id.ToString(), fisher.Id.ToString(), "999");

            // Act
            var result = await _fixture.SendAsync(command);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Categories.NotFound);
        }
    }
}