using FisherTournament.Domain.FisherAggregate.ValueObjects;

namespace FisherTournament.Application.Fishers.Commands.CreateFisher;
public record struct CreateFisherCommandResponse(FisherId Id, string FirstName, string LastName, string DNI);