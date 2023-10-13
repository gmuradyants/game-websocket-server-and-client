using Microsoft.EntityFrameworkCore.Storage;

namespace Game.DataAccess.Repositories.Interfaces;

public interface IGameUnitOfWork
{
    IPlayerRepository PlayerRepository { get; }
    IResourceRepository ResourceRepository { get; }
    IResourceTypeRepository ResourceTypeRepository { get; }
    IGiftRepository GiftRepository { get; }
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously saves changes to the database with a retry mechanism for handling concurrency exceptions.
    /// </summary>
    /// <param name="updateAction">The action to perform that updates the database entities.</param>
    /// <param name="timeOutInMilliseconds">The time in milliseconds to wait before the operation is canceled. Default is 5000 milliseconds.</param>
    Task SaveWithRetryAsync(Action updateAction, int timeOutInMilliseconds = 5000, CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task CommitAsync(CancellationToken cancellationToken = default);
}