using System.Linq.Expressions;

namespace Game.DataAccess.Repositories.Interfaces;

public interface IBaseRepository<TEntity>
{
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity?, bool>> predicate, bool tracking = false, CancellationToken cancellationToken = default);
    Task<ICollection<TEntity>> GetWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity> AddAsync(TEntity item, CancellationToken cancellationToken = default);
}