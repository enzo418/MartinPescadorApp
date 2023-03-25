using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.Application.Tournaments.Commands.AddCategory;
using FisherTournament.Domain.TournamentAggregate.Entities;

namespace FisherTournament.UnitTests.Tournaments.Commands.AddCategory
{
    public class AddCategoryCommandHandlerTests : BaseHandlerTest
    {
        private AddCategoryCommandHandler Handler => new(_contextMock.Object);

        [Fact]
        public async Task Handler_ShouldNot_AddCategory_WhenTournamentDoesNotExist()
        {
            // Arrange
            _contextMock.SetupTournaments(new List<Tournament>());

            var command = new AddCategoryCommand(Guid.Empty.ToString(), "Category 1");

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.NotFound);
        }

        [Fact]
        public async Task Handler_ShouldNot_AddCategory_WhenCategoryAlreadyExists()
        {
            // Arrange
            var tournament = Tournament.Create("Tournament 1",
                                               DateTime.UtcNow.AddDays(1),
                                               DateTime.UtcNow.AddDays(2),
                                               new List<Category>()
                                               {
                                                   Category.Create("Category 1")
                                               });

            _contextMock.SetupTournament(tournament);

            var command = new AddCategoryCommand(tournament.Id.ToString(), "Category 1");

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Categories.AlreadyExists);
        }
    }
}