using Game.BusinessLogicLayer.Core.Exceptions;
using Game.BusinessLogicLayer.Services.Interfaces;
using Game.DataAccess.Context.Entities;
using Game.DataAccess.Repositories.Interfaces;

namespace Game.BusinessLogicLayer.Services.Implementations;

public class PlayerService : IPlayerService
{
    private readonly IGameUnitOfWork _unitOfWork;

    public PlayerService(IGameUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<Player?> GetPlayerByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.PlayerRepository.FirstOrDefaultAsync(p => p.Id == id, cancellationToken: cancellationToken);
    }

    public Task<Player?> GetPlayerByDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        return _unitOfWork.PlayerRepository.FirstOrDefaultAsync(p => p.DeviceId == deviceId, cancellationToken: cancellationToken);
    }

    public async Task<Player> AddPlayerAsync(Player player, CancellationToken cancellationToken = default)
    {
        var addedPlayer = await _unitOfWork.PlayerRepository.AddAsync(player, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return addedPlayer;
    }

    public async Task<Resource> AddOrUpdatePlayerResourceAsync(int playerId, string? resourceType, double resourceValue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resourceType))
            throw new ValidationException("Resource type cannot be empty.");

        var resourceTypeToFind = resourceType.ToLowerInvariant();

        var existingResourceType = await _unitOfWork.ResourceTypeRepository.FirstOrDefaultAsync(r =>
            r!.ResourceTypeName.ToLower() == resourceTypeToFind, cancellationToken: cancellationToken);

        if (existingResourceType is null)
            throw new ValidationException($"Resource type with name '{resourceType}' could not be found.");

        var existingResource = await _unitOfWork.ResourceRepository.FirstOrDefaultAsync(r =>
            r.PlayerId == playerId && r.ResourceTypeId == existingResourceType.Id, true, cancellationToken);

        if (existingResource is null)
        {
            existingResource = await _unitOfWork.ResourceRepository.AddAsync(new Resource
            {
                PlayerId = playerId,
                ResourceTypeId = existingResourceType.Id,
                ResourceValue = resourceValue
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        else
        {
            //This action updates the resource value atomically.
            await _unitOfWork.SaveWithRetryAsync(() => existingResource.ResourceValue += resourceValue, cancellationToken:
                cancellationToken);
        }

        return existingResource;
    }
}