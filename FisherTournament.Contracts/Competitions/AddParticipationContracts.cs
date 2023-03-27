namespace FisherTournament.Contracts.Competitions;

public record struct AddParticipationRequest(
    string FisherId,
    string CompetitionId
);