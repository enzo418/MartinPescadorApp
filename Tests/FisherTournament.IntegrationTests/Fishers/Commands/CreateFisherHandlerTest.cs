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
            using var context = _fixture.TournamentContext;
            var command = new CreateFisherCommand("First", "Last", "12131415");

            // Act
            var result = await _fixture.SendAsync(command);
            var fisher = await context.FindAsync<Fisher>(result.Value.Id);
            var user = context.Set<User>().FirstOrDefault(u => u.FisherId == result.Value.Id);

            // Assert
            result.IsError.Should().BeFalse();
            fisher.Should().NotBeNull();
            user.Should().NotBeNull();
            user!.FirstName.Should().Be("First");
            user.LastName.Should().Be("Last");
        }

        [Fact]
        public async Task Handler_Should_ReturnError_WhenDNIAlreadyExists()
        {
            // Arrange
            using var context = _fixture.TournamentContext;
            var command = new CreateFisherCommand("First", "Last", "12131415");
            var user = User.Create("First", "Last", "12131415", Guid.NewGuid());
            await context.AddAsync(user);
            await context.SaveChangesAsync(default);

            // Act
            var result = await _fixture.SendAsync(command);

            // Assert
            result.IsError.Should().BeTrue();
            result.Errors.First().Should().Be(Errors.Users.DNIAlreadyExists);
        }
    }
}