namespace FisherTournament.Contracts.Competitions;

public record struct AddCompetitionsRequest(
List<AddCompetitionRequest> Competitions
);

public record struct AddCompetitionsResponse(
	List<CompetitionResponse> Competitions);

public record struct CompetitionResponse(
	string Id,
	DateTime StartDateTime,
	DateTime? EndDateTime,
	LocationResponse Location);

public record struct LocationResponse(
	string City,
	string State,
	string Country,
	string Place);

public class AddCompetitionRequest
{
	public DateTime StartDateTime { get; set; }
	public DateTime? EndDateTime { get; set; }
	public CreateLocationRequest Location { get; set; }

	public AddCompetitionRequest(DateTime startDateTime, DateTime? endDateTime, CreateLocationRequest location)
	{
		StartDateTime = startDateTime;
		EndDateTime = endDateTime;
		Location = location;
	}
}

public class CreateLocationRequest
{
	public string City { get; set; } = string.Empty;
	public string State { get; set; } = string.Empty;
	public string Country { get; set; } = string.Empty;
	public string Place { get; set; } = string.Empty;

	public CreateLocationRequest(string city, string state, string country, string place)
	{
		City = city;
		State = state;
		Country = country;
		Place = place;
	}

	public CreateLocationRequest() { }
}
