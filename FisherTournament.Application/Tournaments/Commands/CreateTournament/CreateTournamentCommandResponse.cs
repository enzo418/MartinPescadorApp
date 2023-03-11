namespace FisherTournament.Application.Tournaments.Commands.CreateTournament;

public record struct CreateTournamentCommandResponse(
    Guid Id,
    string Name,
    DateTime startDate,
    DateTime endDate);