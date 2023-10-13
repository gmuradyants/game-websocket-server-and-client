using System.Linq.Expressions;
using Game.DataAccess.Context;
using Game.DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Game.DataAccess.Repositories.Implementations;

public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : class
{
    protected readonly GameDbContext GameDbContext;

    protected BaseRepository(GameDbContext gameDbContext)
    {
        GameDbContext = gameDbContext;
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity?, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default)
    {
        var dbSet = GameDbContext.Set<TEntity>();
        var query = tracking ? dbSet : dbSet.AsNoTracking();
        return await query.FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<TEntity> AddAsync(TEntity item, CancellationToken cancellationToken = default)
    {
        var entity = await GameDbContext.Set<TEntity>().AddAsync(item, cancellationToken);
        return entity.Entity;
    }

    public async Task<ICollection<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await GameDbContext.Set<TEntity>().AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }
}