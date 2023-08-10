
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
                Guid.NewGuid().ToString(),
                "0",
                1);

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
                fisherId,
                "0",
                1);

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
                Guid.NewGuid().ToString(),
                "0",
                1);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [ClassData(typeof(NullEmptyStringTesData))]
        public void Validator_Error_WhenPassedEmptyCategoryId(string categoryId)
        {
            // Arrange
            var command = new AddInscriptionCommand(
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                categoryId,
                1);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CategoryId);
        }
    }
}