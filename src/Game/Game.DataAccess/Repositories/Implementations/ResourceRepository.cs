using Game.DataAccess.Context;
using Game.DataAccess.Context.Entities;
using Game.DataAccess.Repositories.Interfaces;

namespace Game.DataAccess.Repositories.Implementations;

public sealed class ResourceRepository : BaseRepository<Resource>, IResourceRepository
{
    public ResourceRepository(GameDbContext gameDbContext) : base(gameDbContext)
    {
    }
}