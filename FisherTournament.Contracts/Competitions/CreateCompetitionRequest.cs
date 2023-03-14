using System;

namespace FisherTournament.Contracts.Competitions
{
    public record struct AddCompetitionsRequest(
        List<AddCompetitionRequest> Competitions
    );

    public record struct AddCompetitionRequest(
        DateTime StartDateTime,
        DateTime EndDate,
        CreateLocationRequest Location
    );

    public record struct CreateLocationRequest(
        string City,
        string State,
        string Country,
        string Place
    );
}