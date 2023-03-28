namespace FisherTournament.ReadModels.Persistence;

public interface IReadModelsUnitOfWork : IDisposable
{
    ILeaderBoardRepository LeaderBoardRepository { get; }

    void Commit();
}