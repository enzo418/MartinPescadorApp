using FisherTournament.Application.Fishers.Commands.CreateFisher;
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
        var command = new CreateFisherCommand(name, "Last", "12131415");

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
        var command = new CreateFisherCommand("First", name, "12131415");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.LastName);
    }

    [Fact]
    public async Task Validator_Error_WhenPassedEmptyDNI()
    {
        // Arrange
        var command = new CreateFisherCommand("First", "Last", "");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.DNI);
    }

    [Fact]
    public async Task Validator_NoError_WhenPassedValidData()
    {
        // Arrange
        var command = new CreateFisherCommand("First", "Last", "12131415");

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}