using FisherTournament.Application.Competitions.Commands.AddScore;
using FluentValidation.TestHelper;

namespace FisherTournament.UnitTests.Competitions.Commands.AddScore
{
    public class AddScoreCommandValidatorTest
    {
        private readonly AddScoreCommandValidator _validator;

        public AddScoreCommandValidatorTest()
        {
            _validator = new AddScoreCommandValidator();
        }

        [Theory]
        [ClassData(typeof(NullEmptyStringTesData))]
        public async Task Validator_Error_WhenPassedEmptyCompetitionId(string competitionId)
        {
            //
            var command = new AddScoreCommand(
                "FisherId",
                competitionId,
                1);

            //
            var result = await _validator.TestValidateAsync(command);

            //
            result.ShouldHaveValidationErrorFor(c => c.CompetitionId);
        }

        [Theory]
        [ClassData(typeof(NullEmptyStringTesData))]
        public async Task Validator_Error_WhenPassedEmptyFisherId(string fisherId)
        {
            //
            var command = new AddScoreCommand(
                fisherId,
                "CompetitionId",
                1);

            //
            var result = await _validator.TestValidateAsync(command);

            //
            result.ShouldHaveValidationErrorFor(c => c.FisherId);
        }

        [Theory]
        [ClassData(typeof(NegativeNumberTestData))]
        public async Task Validator_Error_WhenPassedInvalidScore(int score)
        {
            //
            var command = new AddScoreCommand(
                "FisherId",
                "CompetitionId",
                score);

            //
            var result = await _validator.TestValidateAsync(command);

            //
            result.ShouldHaveValidationErrorFor(c => c.Score);
        }
    }
}