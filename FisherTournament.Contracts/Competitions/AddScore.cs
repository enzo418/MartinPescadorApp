namespace FisherTournament.Contracts.Competitions
{
    public record struct AddScoreRequest(
        string FisherId,
        int Score
    );
}