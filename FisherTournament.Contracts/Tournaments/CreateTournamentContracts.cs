namespace FisherTournament.Contracts.Tournaments;

public class CreateTournamentRequest
{
    public string Name { get; set; } = default!;
    public DateTime StartDate { get; set; } = default!;
    public DateTime? EndDate { get; set; } = default!;

    public CreateTournamentRequest(string name, DateTime startDate, DateTime? endDate)
    {
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
    }
}

public record struct CreateTournamentResponse(
    string Id,
    string Name,
    DateTime StartDate,
    DateTime? EndDate);