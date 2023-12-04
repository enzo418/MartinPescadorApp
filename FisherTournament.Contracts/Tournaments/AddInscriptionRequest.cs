namespace FisherTournament.Contracts.Tournaments;

public class AddInscriptionRequest
{
	public string FisherId { get; set; }
	public string CategoryId { get; set; }
	public int Number { get; set; }

	public AddInscriptionRequest(string fisherId, string categoryId, int number)
	{
		FisherId = fisherId;
		CategoryId = categoryId;
		Number = number;
	}

	public AddInscriptionRequest()
	{
	}
}