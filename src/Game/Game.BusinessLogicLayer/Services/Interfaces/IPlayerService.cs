using Game.DataAccess.Context.Entities;

namespace Game.BusinessLogicLayer.Services.Interfaces;

public interface IPlayerService
{
    Task<Player?> GetPlayerByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Player?> GetPlayerByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default);
    Task<Player> AddPlayerAsync(Player player, CancellationToken cancellationToken = default);
    Task<Resource> AddOrUpdatePlayerResourceAsync(int playerId, string? resourceType, double resourceValue, CancellationToken cancellationToken = default);
}