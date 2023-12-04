namespace FisherTournament.Contracts.Competitions
{
	public class EditCompetitionRequest
	{
		public DateTime? StartDateTime { get; set; }
		public CreateLocationRequest? Location { get; set; }
		public bool? CompetitionFinishedState { get; set; }

		public EditCompetitionRequest(DateTime? startDateTime, CreateLocationRequest? location, bool? endCompetition)
		{
			StartDateTime = startDateTime;
			Location = location;
			CompetitionFinishedState = endCompetition;
		}

		public EditCompetitionRequest() { }
	}
}
