namespace FisherTournament.Application.Fishers.Commands.CreateFisher;
public record struct CreateFisherCommandResponse(Guid Id, string FirstName, string LastName);