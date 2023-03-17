namespace FisherTournament.Contracts.Competitions
{
    public record struct AddScoreRequest(
        string FisherId, // string works for all the possible id types
        int Score
    );
}