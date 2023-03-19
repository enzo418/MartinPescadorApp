
using FisherTournament.Application.Tournaments.Commands.AddInscription;
using FluentValidation.TestHelper;

namespace FisherTournament.UnitTests.Tournaments.Commands.AddInscription
{
    public class AddInscriptionCommandValidatorTest
    {
        private readonly AddInscriptionCommandValidator _validator;

        public AddInscriptionCommandValidatorTest()
        {
            _validator = new AddInscriptionCommandValidator();
        }

        [Theory]
        [ClassData(typeof(NullEmptyStringTesData))]
        public void Validator_Error_WhenPassedEmptyTournamentId(string tournamentId)
        {
            // Arrange
            var command = new AddInscriptionCommand(
                tournamentId,
                Guid.NewGuid().ToString());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.TournamentId);
        }

        [Theory]
        [ClassData(typeof(NullEmptyStringTesData))]
        public void Validator_Error_WhenPassedEmptyFisherId(string fisherId)
        {
            // Arrange
            var command = new AddInscriptionCommand(
                Guid.NewGuid().ToString(),
                fisherId);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.FisherId);
        }

        [Fact]
        public void Validator_NoError_WhenPassedValidCommand()
        {
            // Arrange
            var command = new AddInscriptionCommand(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}