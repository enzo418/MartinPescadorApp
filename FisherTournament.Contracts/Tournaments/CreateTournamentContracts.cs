namespace FisherTournament.Contracts.Tournaments;

public record struct CreateTournamentRequest(
    string Name,
    DateTime StartDate,
    DateTime EndDate);

public record struct CreateTournamentResponse(
    string Id,
    string Name,
    DateTime StartDate,
    DateTime EndDate);