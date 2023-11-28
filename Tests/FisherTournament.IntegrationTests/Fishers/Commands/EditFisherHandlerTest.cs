using FisherTournament.Application.Fishers.Commands.CreateFisher;
using FisherTournament.Application.Fishers.Commands.EditFisher;
using FisherTournament.Domain.FisherAggregate.ValueObjects;

namespace FisherTournament.IntegrationTests.Fishers.Commands
{
    public class EditFisherHandlerTest : BaseUseCaseTest
    {
        public EditFisherHandlerTest(UseCaseTestsFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public async Task Handler_Should_EditFisher()
        {
            // Arrange
            using var context = _fixture.TournamentContext;

            FisherId fisherId;

            {
                var createCommand = new CreateFisherCommand("First", "Last", "17001915");
                var result = await _fixture.SendAsync(createCommand);

                Assert.False(result.IsError);

                fisherId = result.Value.Id;
            }

            var command = new EditFisherCommand(fisherId, "FirstName", "LastName", "17501915");

            // Act
            var resultEdit = await _fixture.SendAsync(command);

            var fisherEdit = await context.FindAsync<Fisher>(resultEdit.Value.Id);
            var userEdit = context.Set<User>().FirstOrDefault(u => u.FisherId == resultEdit.Value.Id);

            // Assert
            resultEdit.IsError.Should().BeFalse();

            userEdit.Should().NotBeNull();
            userEdit!.FirstName.Should().Be("FirstName");
            userEdit.LastName.Should().Be("LastName");
            userEdit.DNI.Should().Be("17501915");

            fisherEdit.Should().NotBeNull();
            fisherEdit?.Name.Should().Contain("FirstName");
            fisherEdit?.Name.Should().Contain("LastName");
        }

        [Fact]
        public async Task Handler_Should_ReturnError_WhenFisherDoesntExists()
        {
            // Arrange
            using var context = _fixture.TournamentContext;

            var fisherId = FisherId.Create(Guid.NewGuid());

            var command = new EditFisherCommand(fisherId.Value, "FirstName", "LastName", "16131411");

            // Act
            var resultEdit = await _fixture.SendAsync(command);

            // Assert
            resultEdit.IsError.Should().BeTrue();
            resultEdit.Errors.First().Should().Be(Errors.Fishers.NotFound);
        }

        [Fact]
        public async Task Handler_Should_ReturnError_WhenDNIAlreadyExists()
        {
            // Arrange
            using var context = _fixture.TournamentContext;

            FisherId fisherId;

            {
                var user = User.Create("First", "Last", "12131415", Guid.NewGuid());
                await context.AddAsync(user);
                await context.SaveChangesAsync(default);

                var command = new CreateFisherCommand("First", "Last", "11111111");
                var result = await _fixture.SendAsync(command);

                fisherId = result.Value.Id;
            }

            var commandEdit = new EditFisherCommand(fisherId, "FirstName", "LastName", "12131415");

            // Act
            var resultEdit = await _fixture.SendAsync(commandEdit);

            // Assert
            resultEdit.IsError.Should().BeTrue();
            resultEdit.Errors.First().Should().Be(Errors.Users.DNIAlreadyExists);
        }
    }
}