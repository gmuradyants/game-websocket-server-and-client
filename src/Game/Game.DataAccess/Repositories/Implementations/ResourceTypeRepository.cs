using Game.DataAccess.Context;
using Game.DataAccess.Context.Entities;
using Game.DataAccess.Repositories.Interfaces;

namespace Game.DataAccess.Repositories.Implementations;

public sealed class ResourceTypeRepository : BaseRepository<ResourceType>, IResourceTypeRepository
{
    public ResourceTypeRepository(GameDbContext gameDbContext) : base(gameDbContext)
    {
    }
}