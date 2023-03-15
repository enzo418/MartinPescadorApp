namespace FisherTournament.Contracts.Competitions;

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
