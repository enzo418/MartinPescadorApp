namespace FisherTournament.Contracts.Competitions;

public record struct AddCompetitionsRequest(
List<AddCompetitionRequest> Competitions
);

public record struct AddCompetitionsResponse(
    List<CompetitionResponse> Competitions);

public record struct CompetitionResponse(
    string Id,
    DateTime StartDateTime,
    DateTime EndDateTime,
    LocationResponse Location);

public record struct LocationResponse(
    string City,
    string State,
    string Country,
    string Place);

public record struct AddCompetitionRequest(
    DateTime StartDateTime,
    DateTime EndDate,
    CreateLocationRequest Location
);

public record struct CreateLocationRequest(
    string City,
    string State,
    string Country,
    string Place
);
