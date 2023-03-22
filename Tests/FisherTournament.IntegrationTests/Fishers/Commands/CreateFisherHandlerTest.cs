using FisherTournament.Application.Fishers.Commands.CreateFisher;

namespace FisherTournament.IntegrationTests.Fishers.Commands
{
    public class CreateFisherHandlerTest : BaseUseCaseTest
    {
        public CreateFisherHandlerTest(UseCaseTestsFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Handler_Should_CreateFisher()
        {
            // Arrange
            using var context = _fixture.Context;
            var command = new CreateFisherCommand("First", "Last");

            // Act
            var result = await _fixture.SendAsync(command);
            var fisher = await context.FindAsync<Fisher>(result.Value.Id);
            var user = await context.FindAsync<User>(fisher is null ? "" : fisher.UserId);

            // Assert
            result.IsError.Should().BeFalse();
            fisher.Should().NotBeNull();
            user.Should().NotBeNull();
            user!.FirstName.Should().Be("First");
            user.LastName.Should().Be("Last");
        }
    }
}