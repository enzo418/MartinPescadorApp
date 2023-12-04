namespace FisherTournament.Contracts.Tournaments
{
	public class EditInscriptionRequest
	{
		public string FisherId { get; set; }
		public string? CategoryId { get; set; }
		public int? Number { get; set; }

		public EditInscriptionRequest(string fisherId, string? categoryId = null, int? number = null)
		{
			FisherId = fisherId;
			CategoryId = categoryId;
			Number = number;
		}

		public EditInscriptionRequest()
		{
		}
	}
}
