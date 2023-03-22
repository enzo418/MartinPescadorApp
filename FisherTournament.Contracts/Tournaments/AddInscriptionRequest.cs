namespace FisherTournament.Contracts.Tournaments;

public record struct AddInscriptionRequest(
    string FisherId,
    string CategoryId
);