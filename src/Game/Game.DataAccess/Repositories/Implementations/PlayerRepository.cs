using Game.DataAccess.Context;
using Game.DataAccess.Context.Entities;
using Game.DataAccess.Repositories.Interfaces;

namespace Game.DataAccess.Repositories.Implementations;

public sealed class PlayerRepository : BaseRepository<Player>, IPlayerRepository
{
    public PlayerRepository(GameDbContext gameDbContext) : base(gameDbContext)
    {
    }
}