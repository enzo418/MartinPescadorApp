using FisherTournament.Domain.FisherAggregate.ValueObjects;

namespace FisherTournament.Application.Fishers.Queries;

public record struct FisherItem(FisherId Id, string FirstName, string LastName, string DNI);