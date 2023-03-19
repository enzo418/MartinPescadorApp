using FisherTournament.Application.Common.Persistence;
using FisherTournament.Application.Fishers.Commands.CreateFisher;
using FluentValidation;
using FluentValidation.TestHelper;

namespace FisherTournament.UnitTests.Fishers.Commands.CreateFisher;


public class CreateFisherCommandValidatorTest
{
    private readonly CreateFisherCommandValidator _validator;

    public CreateFisherCommandValidatorTest()
    {
        _validator = new CreateFisherCommandValidator();
    }

    [Theory]
    [ClassData(typeof(NullEmptyStringTesData))]
    public async Task Validator_Error_WhenPassedEmptyFirstName(string name)
    {
        // Arrange
        var command = new CreateFisherCommand(name, "Last");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.FirstName);
    }

    [Theory]
    [ClassData(typeof(NullEmptyStringTesData))]
    public async Task Validator_Error_WhenPassedEmptyLastName(string name)
    {
        // Arrange
        var command = new CreateFisherCommand("First", name);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

    [Fact]
    public async Task Validator_NoError_WhenPassedValidData()
    {
        // Arrange
        var command = new CreateFisherCommand("First", "Last");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}