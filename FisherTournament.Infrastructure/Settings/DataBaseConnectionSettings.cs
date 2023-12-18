namespace FisherTournament.Infrastructure.Settings;

public class DataBaseConnectionSettings
{
    public string TournamentDbConnectionString { get; set; } = null!;
    public string ReadModelsDbConnectionString { get; set; } = null!;
}