namespace FisherTournament.Contracts.Fishers;

public record struct CreateFisherRequest(string FirstName, string LastName);

public record struct CreateFisherResponse(
    string Id,
    string FirstName,
    string LastName);
