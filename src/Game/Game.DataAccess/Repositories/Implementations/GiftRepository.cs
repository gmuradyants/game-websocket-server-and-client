using Game.DataAccess.Context;
using Game.DataAccess.Context.Entities;
using Game.DataAccess.Repositories.Interfaces;

namespace Game.DataAccess.Repositories.Implementations;

public sealed class GiftRepository : BaseRepository<Gift>, IGiftRepository
{
    public GiftRepository(GameDbContext gameDbContext) : base(gameDbContext)
    {
    }
}