using FisherTournament.Domain.FisherAggregate.ValueObjects;

namespace FisherTournament.Application.Fishers.Commands.EditFisher;
public record struct EditFisherCommandResponse(FisherId Id, string FirstName, string LastName, string DNI);