using FisherTournament.Domain.TournamentAggregate.ValueObjects;

namespace FisherTournament.Application.Tournaments.Commands.CreateTournament;

public record struct CreateTournamentCommandResponse(
    TournamentId Id,
    string Name,
    DateTime StartDate,
    DateTime? EndDate);