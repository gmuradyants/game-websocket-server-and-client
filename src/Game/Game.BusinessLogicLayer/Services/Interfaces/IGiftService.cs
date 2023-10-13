using Game.DataAccess.Context.Entities;

namespace Game.BusinessLogicLayer.Services.Interfaces;

public interface IGiftService
{
    Task<Gift> SendGiftAsync(int friendPlayerId, int senderPlayerId, string? resourceType, double resourceValue, CancellationToken cancellationToken = default);
}