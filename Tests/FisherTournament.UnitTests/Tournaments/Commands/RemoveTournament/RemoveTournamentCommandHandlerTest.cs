using FisherTournament.Application.Tournaments.Commands.RemoveTournament;
using FisherTournament.Domain.CompetitionAggregate.ValueObjects;
using FisherTournament.Domain.FisherAggregate.ValueObjects;
using FisherTournament.Domain.TournamentAggregate.Entities;

namespace FisherTournament.UnitTests.Tournaments.Commands.RemoveTournament
{
    public class RemoveTournamentCommandHandlerTest : BaseHandlerTest
    {
        private RemoveTournamentCommandHandler Handler => new(_contextMock.Object);

        public RemoveTournamentCommandHandlerTest()
        {
            _dateTimeProviderMock.SetupGet(p => p.Now).Returns(DateTime.UtcNow);
        }

        [Fact]
        public async Task Handler_ShouldNot_RemoveTournament_WhenTournamentDoesNotExist()
        {
            // Arrange
            _contextMock.SetupTournaments(new List<Tournament>());

            var command = new RemoveTournamentCommand(Guid.Empty.ToString());

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.NotFound);
        }

        [Fact]
        public async Task Handler_ShouldNot_RemoveTournament_WhenTournamentIsNotOver()
        {
            // Arrange
            var tournament = Tournament.Create("Tournament 1",
                                               _dateTimeProviderMock.Object.Now.AddDays(1),
                                               null,
                                               new List<Category>());

            _contextMock.SetupTournament(tournament);

            var command = new RemoveTournamentCommand(tournament.Id.ToString());

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.CannotBeDeleted);
        }

        [Fact]
        public async Task Handler_ShouldNot_RemoveTournament_WhenTournamentHasInscriptions()
        {
            // Arrange
            var categoryMock = MockCategory(1);

            var tournament = Tournament.Create("Tournament 1",
                                               _dateTimeProviderMock.Object.Now.AddDays(-2),
                                               null,
                                               new List<Category>() { categoryMock.Object });

            var inscRes = tournament.AddInscription(FisherId.Create(Guid.NewGuid().ToString()).Value,
                                                    categoryMock.Object.Id,
                                                    1,
                                                    _dateTimeProviderMock.Object);

            _contextMock.SetupTournament(tournament);

            var command = new RemoveTournamentCommand(tournament.Id.ToString());

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            inscRes.IsError.Should().BeFalse();
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.CannotBeDeleted);
        }

        [Fact]
        public async Task Handler_ShouldNot_RemoveTournament_WhenTournamentHasCompetitions()
        {
            // Arrange
            var categoryMock = MockCategory(1);

            var tournament = Tournament.Create("Tournament 1",
                                               _dateTimeProviderMock.Object.Now.AddDays(-2),
                                               _dateTimeProviderMock.Object.Now.AddDays(-1),
                                               new List<Category>() { categoryMock.Object });

            tournament.AddCompetition(CompetitionId.Create(Guid.NewGuid()).Value);

            _contextMock.SetupTournament(tournament);

            var command = new RemoveTournamentCommand(tournament.Id.ToString());

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.CannotBeDeleted);
        }

        [Fact]
        public async Task Handler_Should_RemoveTournament_WhenTournamentHasNoData()
        {
            // Arrange
            var tournament = Tournament.Create("Tournament 1",
                                               _dateTimeProviderMock.Object.Now.AddDays(-2),
                                               _dateTimeProviderMock.Object.Now.AddDays(-1),
                                               new List<Category>());

            _contextMock.SetupTournament(tournament);

            var command = new RemoveTournamentCommand(tournament.Id.ToString());

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeFalse();
        }
    }
}
