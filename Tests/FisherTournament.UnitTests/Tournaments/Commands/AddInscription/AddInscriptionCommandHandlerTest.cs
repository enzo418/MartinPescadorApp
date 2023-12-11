using FisherTournament.Application.Tournaments.Commands.AddInscription;
using FisherTournament.Domain.TournamentAggregate.Entities;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.UnitTests.Tournaments.Commands
{
    public class AddInscriptionCommandHandlerTest : BaseHandlerTest
    {
        private AddInscriptionCommandHandler Handler => new(_contextMock.Object, _dateTimeProviderMock.Object);

        public AddInscriptionCommandHandlerTest()
        {
            _dateTimeProviderMock.SetupGet(p => p.Now).Returns(DateTime.UtcNow);
        }

        [Fact]
        public async Task Handler_ShouldNot_AddInscription_WhenTournamentDoesNotExist()
        {
            // Arrange
            var fisher = Fisher.Create("First Name", "Last Name");

            this._contextMock.SetupTournaments(new List<Tournament>())
                             .SetupFisher(fisher);

            const int number = 1;
            var command = new AddInscriptionCommand(Guid.Empty.ToString(),
                                                    fisher.Id.ToString(),
                                                    CategoryId.Create(1).Value.ToString(),
                                                    number);

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.NotFound);
        }

        [Fact]
        public async Task Handler_ShouldNot_AddInscription_WhenFisherDoesNotExist()
        {
            // Arrange
            var categoryMock = MockCategory(1);

            var tournament = Tournament.Create("Tournament 1",
                                               _dateTimeProviderMock.Object.Now.AddDays(1),
                                               _dateTimeProviderMock.Object.Now.AddDays(2),
                                               new List<Category>() { categoryMock.Object });

            this._contextMock.SetupTournament(tournament)
                             .SetupFishers(new List<Fisher>());

            const int number = 1;
            var command = new AddInscriptionCommand(tournament.Id.ToString(),
                                                    Guid.Empty.ToString(),
                                                    categoryMock.Object.Id.ToString(),
                                                    number);

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Fishers.NotFound);
        }

        [Fact]
        public async Task Handler_ShouldNot_AddInscription_WhenFisherIsAlreadyEnrolled()
        {
            // Arrange
            var categoryMock = MockCategory(1);

            var tournament = Tournament.Create("Tournament 1",
                                               _dateTimeProviderMock.Object.Now.AddDays(1),
                                               _dateTimeProviderMock.Object.Now.AddDays(2),
                                               new List<Category>() { categoryMock.Object });

            var fisher = Fisher.Create("First Name", "Last Name");

            const int number = 1;
            tournament.AddInscription(fisher.Id,
                                      categoryMock.Object.Id,
                                      number,
                                      _dateTimeProviderMock.Object);

            this._contextMock.SetupTournament(tournament)
                             .SetupFisher(fisher);

            var command = new AddInscriptionCommand(tournament.Id.ToString(),
                                                    fisher.Id.ToString(),
                                                    categoryMock.Object.Id.ToString(),
                                                    number);

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.InscriptionAlreadyExists);
        }

        [Fact]
        public async Task Handler_ShouldNot_AddInscription_WhenTournamentIsOver()
        {
            // Arrange
            var categoryMock = MockCategory(1);

            var tournament = Tournament.Create("Tournament 1",
                                               _dateTimeProviderMock.Object.Now.AddDays(-2),
                                               _dateTimeProviderMock.Object.Now.AddDays(-1),
                                               new List<Category>() { categoryMock.Object });

            var fisher = Fisher.Create("First Name", "Last Name");

            this._contextMock.SetupTournament(tournament)
                             .SetupFisher(fisher);

            const int number = 1;
            var command = new AddInscriptionCommand(tournament.Id.ToString(),
                                                    fisher.Id.ToString(),
                                                    categoryMock.Object.Id.ToString(),
                                                    number);

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.IsOver);
        }

        [Fact]
        public async Task Handler_ShouldNot_AddInscription_WhenCategoryDoesNotExist()
        {
            // Arrange
            var tournament = Tournament.Create("Tournament 1",
                                               _dateTimeProviderMock.Object.Now.AddDays(1),
                                               _dateTimeProviderMock.Object.Now.AddDays(2),
                                               new List<Category>());
            var fisher = Fisher.Create("First Name", "Last Name");

            this._contextMock.SetupTournament(tournament)
                             .SetupFisher(fisher);

            const int number = 1;
            var command = new AddInscriptionCommand(tournament.Id.ToString(),
                                                    fisher.Id.ToString(),
                                                    CategoryId.Create(1).Value.ToString(),
                                                    number);

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Categories.NotFound);
        }
    }
}