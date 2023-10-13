using Game.DataAccess.Context;
using Game.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Game.DataAccess.Repositories.Implementations;

public sealed class GameUnitOfWork : IGameUnitOfWork
{
    private readonly GameDbContext _gameDbContext;

    public IPlayerRepository PlayerRepository { get; }
    public IResourceRepository ResourceRepository { get; }
    public IResourceTypeRepository ResourceTypeRepository { get; }
    public IGiftRepository GiftRepository { get; }

    public GameUnitOfWork(GameDbContext gameDbContext, 
        IPlayerRepository playerRepository, 
        IResourceRepository resourceRepository, 
        IResourceTypeRepository resourceTypeRepository,
        IGiftRepository giftRepository)
    {
        _gameDbContext = gameDbContext;
        PlayerRepository = playerRepository;
        ResourceRepository = resourceRepository;
        ResourceTypeRepository = resourceTypeRepository;
        GiftRepository = giftRepository;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _gameDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveWithRetryAsync(Action updateAction, int timeOutInMilliseconds = 5000, CancellationToken cancellationToken = default)
    {
        var timeoutCancelToken = new CancellationTokenSource(timeOutInMilliseconds);

        var cancelToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCancelToken.Token).Token;

        bool saveFailed;
       
        do
        {
            saveFailed = false;

            updateAction();

            try
            {
                await _gameDbContext.SaveChangesAsync(cancelToken);
            }
            catch (DbUpdateConcurrencyException e)
            {
                saveFailed = true;
                await e.Entries.Single().ReloadAsync(cancelToken);
            }
        } while (saveFailed);
    }

    public Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return _gameDbContext.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _gameDbContext.Database.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await _gameDbContext.Database.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}