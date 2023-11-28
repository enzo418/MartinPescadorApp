namespace FisherTournament.Contracts.Fishers;

public class CreateFisherRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string DNI { get; set; } = default!;

    public CreateFisherRequest(string firstName, string lastName, string dNI)
    {
        FirstName = firstName;
        LastName = lastName;
        DNI = dNI;
    }
}

public record struct CreateFisherResponse(
    string Id,
    string FirstName,
    string LastName,
    string DNI);
