using FisherTournament.Application.Fishers.Commands.CreateFisher;
using FisherTournament.Application.Fishers.Queries;

namespace FisherTournament.IntegrationTests.Fishers.Queries
{
    public class GetFisherQueryTest : BaseUseCaseTest
    {
        public GetFisherQueryTest(UseCaseTestsFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Handler_Should_ReturnFisher()
        {
            // Arrange
            using var context = _fixture.TournamentContext;
            var command = new CreateFisherCommand("first", "last", "10301587");
            var fisher = await _fixture.SendAsync(command);
            Assert.False(fisher.IsError);

            var query = new GetFisherQuery(fisher.Value.Id.ToString());

            // Act
            var result = await _fixture.SendAsync(query);

            // Assert
            result.IsError.Should().BeFalse();
            result.Value.Id.Should().Be(fisher.Value.Id);
            result.Value.FirstName.Should().Be("first");
            result.Value.LastName.Should().Be("last");
            result.Value.DNI.Should().Be("10301587");
        }

        [Fact]
        public async Task Handler_Should_ReturnError_WhenFisherDoesNotExist()
        {
            // Arrange
            using var context = _fixture.TournamentContext;
            var query = new GetFisherQuery(Guid.NewGuid().ToString());

            // Act
            var result = await _fixture.SendAsync(query);

            // Assert
            result.IsError.Should().BeTrue();
            result.Errors.First().Should().Be(Errors.Fishers.NotFound);
        }

        [Fact]
        public async Task Handler_Should_ReturnError_WhenFisherIdIsNotValid()
        {
            // Arrange
            using var context = _fixture.TournamentContext;
            var query = new GetFisherQuery("not-valid-id");

            // Act
            var result = await _fixture.SendAsync(query);

            // Assert
            result.IsError.Should().BeTrue();
            result.Errors.First().Should().Be(Errors.Id.NotValidWithProperty(nameof(GetFisherQuery.FisherId)));
        }
    }
}
