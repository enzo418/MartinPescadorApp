using FisherTournament.Application.Tournaments.Commands.AddCategory;
using FluentValidation.TestHelper;

namespace FisherTournament.UnitTests.Tournaments.Commands
{
    public class AddCategoryCommandValidatorTest
    {
        private readonly AddCategoryCommandValidator _validator;

        public AddCategoryCommandValidatorTest()
        {
            _validator = new AddCategoryCommandValidator();
        }

        [Fact]
        public async Task Validator_NoError_WhenPassedValidData()
        {
            //
            var command = new AddCategoryCommand(
                "TournamentId",
                "CategoryName");

            //
            var result = await _validator.TestValidateAsync(command);

            //
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [ClassData(typeof(NullEmptyStringTesData))]
        public async Task Validator_Error_WhenPassedEmptyTournamentId(string tournamentId)
        {
            //
            var command = new AddCategoryCommand(
                tournamentId,
                "CategoryName");

            //
            var result = await _validator.TestValidateAsync(command);

            //
            result.ShouldHaveValidationErrorFor(c => c.TournamentId);
        }

        [Theory]
        [ClassData(typeof(NullEmptyStringTesData))]
        public async Task Validator_Error_WhenPassedEmptyCategoryName(string categoryName)
        {
            //
            var command = new AddCategoryCommand(
                "TournamentId",
                categoryName);

            //
            var result = await _validator.TestValidateAsync(command);

            //
            result.ShouldHaveValidationErrorFor(c => c.Name);
        }
    }
}