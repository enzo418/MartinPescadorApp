using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.Application.Common.Persistence;
using FisherTournament.Application.Tournaments.Commands.AddInscription;
using FisherTournament.Domain.Common.Provider;
using FisherTournament.Domain.FisherAggregate;
using FisherTournament.Domain.TournamentAggregate;
using FisherTournament.Domain.TournamentAggregate.Entities;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.Domain.UserAggregate.ValueObjects;
using MockQueryable.Moq;
using Moq;

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
            var fisher = Fisher.Create(UserId.Create(Guid.Empty).Value);

            this._contextMock.SetupTournaments(new List<Tournament>())
                             .SetupFisher(fisher);

            var command = new AddInscriptionCommand(Guid.Empty.ToString(),
                                                    fisher.Id.ToString(),
                                                    CategoryId.Create(1).Value.ToString());

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

            var command = new AddInscriptionCommand(tournament.Id.ToString(),
                                                    Guid.Empty.ToString(),
                                                    categoryMock.Object.Id.ToString());

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

            var fisher = Fisher.Create(UserId.Create(Guid.Empty).Value);

            tournament.AddInscription(fisher.Id,
                                      categoryMock.Object.Id,
                                      _dateTimeProviderMock.Object);

            this._contextMock.SetupTournament(tournament)
                             .SetupFisher(fisher);

            var command = new AddInscriptionCommand(tournament.Id.ToString(),
                                                    fisher.Id.ToString(),
                                                    categoryMock.Object.Id.ToString());

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

            var fisher = Fisher.Create(UserId.Create(Guid.Empty).Value);

            this._contextMock.SetupTournament(tournament)
                             .SetupFisher(fisher);

            var command = new AddInscriptionCommand(tournament.Id.ToString(),
                                                    fisher.Id.ToString(),
                                                    categoryMock.Object.Id.ToString());

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
            var fisher = Fisher.Create(UserId.Create(Guid.Empty).Value);

            this._contextMock.SetupTournament(tournament)
                             .SetupFisher(fisher);

            var command = new AddInscriptionCommand(tournament.Id.ToString(),
                                                    fisher.Id.ToString(),
                                                    CategoryId.Create(1).Value.ToString());

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Categories.NotFound);
        }
    }
}