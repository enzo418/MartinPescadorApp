namespace FisherTournament.Contracts.Competitions
{
	public class AddScoreRequest
	{
		public string FisherId { get; set; } = null!;
		public int Score { get; set; }

		public AddScoreRequest(string fisherId, int score)
		{
			FisherId = fisherId;
			Score = score;
		}

		public AddScoreRequest()
		{
		}
	}
}