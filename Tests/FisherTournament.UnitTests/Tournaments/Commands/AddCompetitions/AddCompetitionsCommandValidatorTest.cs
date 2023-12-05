using FisherTournament.Application.Common.Resources;
using FisherTournament.Application.Tournaments.Commands.AddCompetitions;
using FisherTournament.Domain.Common.Provider;
using FluentValidation.TestHelper;

namespace FisherTournament.UnitTests.Tournaments.Commands
{
	public class AddCompetitionsCommandValidatorTest
	{
		private readonly Mock<IDateTimeProvider> _dateTimeProvider;
		private readonly AddCompetitionsCommandValidation _validator;

		public AddCompetitionsCommandValidatorTest()
		{
			_dateTimeProvider = new Mock<IDateTimeProvider>();
			_dateTimeProvider.Setup(d => d.Now).Returns(DateTime.UtcNow);
			_validator = new AddCompetitionsCommandValidation(new CompetitionCommandValidation());
		}

		[Fact]
		public async Task Validator_NoError_WhenPassedValidData()
		{
			//
			var command = new AddCompetitionsCommand(
				"TournamentId",
				new List<AddCompetitionCommand>
				{
					new AddCompetitionCommand(
						_dateTimeProvider.Object.Now.AddDays(1),
						new CompetitionLocationResource("City",
						"State",
						"Country",
						"Place"))
				});

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
			var command = new AddCompetitionsCommand(
				tournamentId,
				new List<AddCompetitionCommand>
				{
					new AddCompetitionCommand(
						_dateTimeProvider.Object.Now.AddDays(1),
						new CompetitionLocationResource("City",
						"State",
						"Country",
						"Place"))
				});

			//
			var result = await _validator.TestValidateAsync(command);

			// 
			result.ShouldHaveValidationErrorFor(c => c.TournamentId);
		}

		[Fact]
		public async Task Validator_Ok_WhenPassedStartDateBeforeNow()
		{
			//
			var command = new AddCompetitionsCommand(
				"TournamentId",
				new List<AddCompetitionCommand>
				{
					new AddCompetitionCommand(
						_dateTimeProvider.Object.Now.AddDays(-1),
						new CompetitionLocationResource("City",
						"State",
						"Country",
						"Place"))
				});

			//
			var result = await _validator.TestValidateAsync(command);

			// 
			result.ShouldNotHaveAnyValidationErrors();
		}

		[Fact]
		public async Task Validator_Error_WhenPassedStartDateNotUtc()
		{
			//
			var command = new AddCompetitionsCommand(
				"TournamentId",
				new List<AddCompetitionCommand>
				{
					new AddCompetitionCommand(
						DateTime.Now.AddDays(1),
						new CompetitionLocationResource("City",
						"State",
						"Country",
						"Place"))
				});

			//
			var result = await _validator.TestValidateAsync(command);

			// 
			result.ShouldHaveAnyValidationError();
		}

		[Fact]
		public async Task Validator_Error_WhenPassedEmptyCompetitions()
		{
			//
			var command = new AddCompetitionsCommand(
				"TournamentId",
				new List<AddCompetitionCommand>());

			//
			var result = await _validator.TestValidateAsync(command);

			// 
			result.ShouldHaveAnyValidationError();
		}

		// [Theory]
		// [ClassData(typeof(NullEmptyStringTesData))]
		// public async Task Validator_Error_WhenPassedEmptyCity(string city)
		// {
		//     //
		//     var command = new AddCompetitionsCommand(
		//         "TournamentId",
		//         new List<AddCompetitionCommand>
		//         {
		//             new AddCompetitionCommand(
		//                 _dateTimeProvider.Object.Now.AddDays(1),
		//                 city,
		//                 "State",
		//                 "Country",
		//                 "Place")
		//         });

		//     //
		//     var result = await _validator.TestValidateAsync(command);

		//     // 
		//     result.ShouldHaveAnyValidationError();
		// }

		// [Theory]
		// [ClassData(typeof(NullEmptyStringTesData))]
		// public async Task Validator_Error_WhenPassedEmptyState(string state)
		// {
		//     //
		//     var command = new AddCompetitionsCommand(
		//         "TournamentId",
		//         new List<AddCompetitionCommand>
		//         {
		//             new AddCompetitionCommand(
		//                 _dateTimeProvider.Object.Now.AddDays(1),
		//                 "City",
		//                 state,
		//                 "Country",
		//                 "Place")
		//         });

		//     //
		//     var result = await _validator.TestValidateAsync(command);

		//     // 
		//     result.ShouldHaveAnyValidationError();
		// }

		// [Theory]
		// [ClassData(typeof(NullEmptyStringTesData))]
		// public async Task Validator_Error_WhenPassedEmptyCountry(string country)
		// {
		//     //
		//     var command = new AddCompetitionsCommand(
		//         "TournamentId",
		//         new List<AddCompetitionCommand>
		//         {
		//             new AddCompetitionCommand(
		//                 _dateTimeProvider.Object.Now.AddDays(1),
		//                 "City",
		//                 "State",
		//                 country,
		//                 "Place")
		//         });

		//     //
		//     var result = await _validator.TestValidateAsync(command);

		//     // 
		//     result.ShouldHaveAnyValidationError();
		// }

		// [Theory]
		// [ClassData(typeof(NullEmptyStringTesData))]
		// public async Task Validator_Error_WhenPassedEmptyPlace(string place)
		// {
		//     //
		//     var command = new AddCompetitionsCommand(
		//         "TournamentId",
		//         new List<AddCompetitionCommand>
		//         {
		//             new AddCompetitionCommand(
		//                 _dateTimeProvider.Object.Now.AddDays(1),
		//                 "City",
		//                 "State",
		//                 "Country",
		//                 place)
		//         });

		//     //
		//     var result = await _validator.TestValidateAsync(command);

		//     // 
		//     result.ShouldHaveAnyValidationError();
		// }
	}
}