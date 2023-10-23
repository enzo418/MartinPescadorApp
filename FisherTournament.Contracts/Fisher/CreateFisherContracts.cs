namespace FisherTournament.Contracts.Fishers;

public class CreateFisherRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;

    public CreateFisherRequest(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}

public record struct CreateFisherResponse(
    string Id,
    string FirstName,
    string LastName);
