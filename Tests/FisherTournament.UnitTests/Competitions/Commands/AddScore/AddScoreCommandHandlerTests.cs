using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FisherTournament.Application.Competitions.Commands.AddScore;
using FisherTournament.Domain.CompetitionAggregate.Entities;
using FisherTournament.Domain.TournamentAggregate.Entities;
using FisherTournament.Domain.TournamentAggregate.ValueObjects;
using FisherTournament.Domain.UserAggregate.ValueObjects;

namespace FisherTournament.UnitTests.Competitions.Commands.AddScore
{
    public class AddScoreCommandHandlerTests : BaseHandlerTest
    {
        private AddScoreCommandHandler Handler => new(_contextMock.Object, _dateTimeProviderMock.Object);

        private Location FakeLocation => Location.Create("city", "state", "country", "place");

        public AddScoreCommandHandlerTests()
        {
            _dateTimeProviderMock.Setup(x => x.Now).Returns(DateTime.UtcNow);
        }

        [Fact]
        public async Task Handler_ShouldNot_AddScore_WhenCompetitionDoesNotExist()
        {
            // Arrange
            var fisher = Fisher.Create(UserId.Create(Guid.Empty).Value);

            _contextMock.SetupCompetitions(new List<Competition>())
                        .SetupFisher(fisher);

            var command = new AddScoreCommand(fisher.Id.ToString(),
                                              Guid.Empty.ToString(),
                                              1);

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Competitions.NotFound);
        }

        [Fact]
        public async Task Handler_ShouldNot_AddScore_WhenFisherDoesNotExist()
        {
            // Arrange
            Tournament tournament = GetFakeTournament();
            var competition = Competition.Create(_dateTimeProviderMock.Object.Now.AddDays(1),
                                                 tournament.Id,
                                                 FakeLocation);

            _contextMock.SetupCompetition(competition)
                        .SetupFishers(new List<Fisher>())
                        .SetupTournament(tournament);

            var command = new AddScoreCommand(Guid.Empty.ToString(),
                                              competition.Id.ToString(),
                                              1);

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Fishers.NotFound);
        }

        [Fact]
        public async Task Handler_ShouldNot_AddScore_WhenCompetitionHasNotStarted()
        {
            // Arrange
            var fisher = Fisher.Create(UserId.Create(Guid.Empty).Value);
            Tournament tournament = GetFakeTournament();
            var competition = Competition.Create(_dateTimeProviderMock.Object.Now.AddDays(1),
                                                 tournament.Id,
                                                 FakeLocation);

            tournament.AddInscription(fisher.Id,
                                      tournament.Categories.First().Id,
                                      _dateTimeProviderMock.Object);

            _contextMock.SetupCompetition(competition)
                        .SetupFisher(fisher)
                        .SetupTournament(tournament);

            var command = new AddScoreCommand(fisher.Id.ToString(),
                                              competition.Id.ToString(),
                                              1);

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Competitions.HasNotStarted);
        }

        [Fact]
        public async Task Handler_ShouldNot_AddScore_WhenCompetitionIsFinished()
        {
            // Arrange
            var fisher = Fisher.Create(UserId.Create(Guid.Empty).Value);
            Tournament tournament = GetFakeTournament();

            tournament.AddInscription(fisher.Id,
                                      tournament.Categories.First().Id,
                                      _dateTimeProviderMock.Object);

            var competition = Competition.Create(_dateTimeProviderMock.Object.Now.AddDays(-1),
                                                 tournament.Id,
                                                 FakeLocation);

            competition.EndCompetition(_dateTimeProviderMock.Object);

            _contextMock.SetupCompetition(competition)
                        .SetupFisher(fisher)
                        .SetupTournament(tournament);

            var command = new AddScoreCommand(fisher.Id.ToString(),
                                              competition.Id.ToString(),
                                              1);

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Competitions.HasEnded);
        }

        [Fact]
        public async Task Handler_ShouldNot_AddScore_WhenFisherIsNotEnrolled()
        {
            // Arrange
            var fisher = Fisher.Create(UserId.Create(Guid.Empty).Value);
            Tournament tournament = GetFakeTournament();

            var competition = Competition.Create(_dateTimeProviderMock.Object.Now.AddDays(1),
                                                 tournament.Id,
                                                 FakeLocation);

            _contextMock.SetupCompetition(competition)
                        .SetupFisher(fisher)
                        .SetupTournament(tournament);

            var command = new AddScoreCommand(fisher.Id.ToString(),
                                              competition.Id.ToString(),
                                              1);

            // Act
            var result = await Handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsError.Should().BeTrue();
            result.FirstError.Should().Be(Errors.Tournaments.NotEnrolled);
        }

        private Tournament GetFakeTournament()
        {
            var category = MockCategory(1);

            return Tournament.Create("name",
                                     _dateTimeProviderMock.Object.Now.AddDays(1),
                                     _dateTimeProviderMock.Object.Now.AddDays(2),
                                     new List<Category>() { category.Object });
        }
    }
}