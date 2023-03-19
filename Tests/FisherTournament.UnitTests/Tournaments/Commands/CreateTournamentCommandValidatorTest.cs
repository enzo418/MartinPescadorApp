using FisherTournament.Application.Tournaments.Commands.AddInscription;
using FisherTournament.Application.Tournaments.Commands.CreateTournament;
using FisherTournament.Domain.Common.Provider;
using FluentValidation.TestHelper;
using Moq;

namespace FisherTournament.UnitTests.Tournaments.Commands.CreateTournament;

public class CreateTournamentCommandValidatorTest
{
    private readonly Mock<IDateTimeProvider> _dateTimeProvider;
    private readonly CreateTournamentCommandValidator _validator;

    public CreateTournamentCommandValidatorTest()
    {
        _dateTimeProvider = new Mock<IDateTimeProvider>();
        _dateTimeProvider.Setup(d => d.Now).Returns(DateTime.UtcNow);
        // --- validator dependencies need to be defined before injected
        _validator = new CreateTournamentCommandValidator(_dateTimeProvider.Object);
    }

    [Theory]
    [ClassData(typeof(NullEmptyStringTesData))]
    public async Task Validator_Error_WhenPassedEmptyName(string name)
    {
        // Arrange
        var command = new CreateTournamentCommand(
            name,
            _dateTimeProvider.Object.Now,
            _dateTimeProvider.Object.Now.AddDays(1));

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.Name);
    }

    [Fact]
    public async Task Validator_Error_WhenPassedStartDateBeforeNow()
    {
        // Arrange
        var command = new CreateTournamentCommand(
            "Name",
            _dateTimeProvider.Object.Now.AddDays(-1),
            _dateTimeProvider.Object.Now.AddDays(1));

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        // any because validator might validate it in start or end date
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task Validator_Error_WhenPassedEndDateBeforeStartDate()
    {
        // Arrange
        var command = new CreateTournamentCommand(
            "Name",
            _dateTimeProvider.Object.Now.AddDays(1),
            _dateTimeProvider.Object.Now);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveAnyValidationError();
    }

    [Fact]
    public async Task Validator_Error_WhenPassedStartDateNotUtc()
    {
        // Arrange
        var command = new CreateTournamentCommand(
            "Name",
            DateTime.Now,
            _dateTimeProvider.Object.Now.AddDays(1));

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.StartDate);
    }

    [Fact]
    public async Task Validator_Error_WhenPassedEndDateNotUtc()
    {
        // Arrange
        var command = new CreateTournamentCommand(
            "Name",
            _dateTimeProvider.Object.Now.AddDays(1),
            DateTime.Now);

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldHaveValidationErrorFor(c => c.EndDate);
    }

    //
    [Fact]
    public async Task Validator_NoError_WhenPassedValidData()
    {
        // Arrange
        var command = new CreateTournamentCommand(
            "Name",
            _dateTimeProvider.Object.Now.AddDays(1),
            _dateTimeProvider.Object.Now.AddDays(2));

        // Act
        var result = await _validator.TestValidateAsync(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}