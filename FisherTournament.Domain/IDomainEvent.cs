using MediatR;

namespace FisherTournament.Domain;

// enum save before/after
public enum DispatchOrder
{
    BeforeSave,
    AfterSave
}

public interface IDomainEvent : INotification
{
    DispatchOrder SaveState { get; init; }
}