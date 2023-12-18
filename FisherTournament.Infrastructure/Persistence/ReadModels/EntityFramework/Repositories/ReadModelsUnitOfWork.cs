using FisherTournament.ReadModels.Persistence;

namespace FisherTournament.Infrastructure.Persistence.ReadModels.EntityFramework.Repositories;

public class ReadModelsUnitOfWork : IReadModelsUnitOfWork
{
    private readonly ReadModelsDbContext _dbContext;

    public ILeaderBoardRepository LeaderBoardRepository { get; }
    
    public void Commit()
    {
        _dbContext.SaveChanges();
    }
    
    public ReadModelsUnitOfWork(ReadModelsDbContext dbContext)
    {
        _dbContext = dbContext;
        LeaderBoardRepository = new LeaderBoardRepository(_dbContext);
    }
    
    private bool _disposed = false;
    
    private void Dispose(bool disposing)
    {
        if (!this._disposed)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }
        }
        this._disposed = true;
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}