namespace FisherTournament.Contracts.Fishers;

public class EditFisherRequest
{
	public string FirstName { get; set; } = default!;
	public string LastName { get; set; } = default!;
	public string DNI { get; set; } = default!;

	public EditFisherRequest(string firstName, string lastName, string dNI)
	{
		FirstName = firstName;
		LastName = lastName;
		DNI = dNI;
	}
}

public record struct EditFisherResponse(
	string Id,
	string FirstName,
	string LastName,
	string DNI);
