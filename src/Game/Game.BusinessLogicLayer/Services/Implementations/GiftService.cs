using Game.BusinessLogicLayer.Core.Exceptions;
using Game.BusinessLogicLayer.Services.Interfaces;
using Game.DataAccess.Context.Entities;
using Game.DataAccess.Repositories.Interfaces;

namespace Game.BusinessLogicLayer.Services.Implementations;

public class GiftService : IGiftService
{
    private readonly IGameUnitOfWork _unitOfWork;

    public GiftService(IGameUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Gift> SendGiftAsync(int friendPlayerId, int senderPlayerId, string? resourceType, double resourceValue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resourceType))
            throw new ValidationException("Resource type cannot be empty.");

        var players = await _unitOfWork.PlayerRepository.GetWhereAsync(p => new[] { friendPlayerId, senderPlayerId }.Contains(p.Id),
            cancellationToken: cancellationToken);

        var playerIds = players.Select(_ => _.Id).ToList();

        if (!playerIds.Contains(friendPlayerId))
            throw new ValidationException($"Friend player with ID {friendPlayerId} could not be found.");

        if (!playerIds.Contains(senderPlayerId))
            throw new InvalidOperationException("The Sender player does not exist.");

        var existingResourceType = await _unitOfWork.ResourceTypeRepository.FirstOrDefaultAsync(r =>
            r!.ResourceTypeName.ToLower() == resourceType.ToLowerInvariant(), cancellationToken: cancellationToken);

        if (existingResourceType is null)
            throw new ValidationException($"Resource type with name '{resourceType}' could not be found.");

        var existingSenderResource = await _unitOfWork.ResourceRepository.FirstOrDefaultAsync(r =>
            r.PlayerId == senderPlayerId && r.ResourceTypeId == existingResourceType.Id, true, cancellationToken);

        if (existingSenderResource is null || existingSenderResource.ResourceValue < resourceValue)
        {
            throw new ValidationException($"Player does not have enough '{resourceType}' to send a gift.");
        }

        var existingFriendResource = await _unitOfWork.ResourceRepository.FirstOrDefaultAsync(r => r.PlayerId == friendPlayerId
            && r.ResourceTypeId == existingResourceType.Id, true, cancellationToken);

        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        //These operations are atomic and thread safe, it atomically deducts a resourceValue from the sender's resource and adds it to the friend's resource.
        if (existingFriendResource is null)
        {
            await _unitOfWork.SaveWithRetryAsync(() => existingSenderResource.ResourceValue -= resourceValue, cancellationToken:
                cancellationToken);

            _ = await _unitOfWork.ResourceRepository.AddAsync(new Resource
            {
                PlayerId = friendPlayerId,
                ResourceTypeId = existingResourceType.Id,
                ResourceValue = resourceValue
            }, cancellationToken);
        }
        else
        {
            await _unitOfWork.SaveWithRetryAsync(
                () => { existingSenderResource.ResourceValue -= resourceValue; },
                cancellationToken: cancellationToken);

            await _unitOfWork.SaveWithRetryAsync(() =>
            {
                existingFriendResource.ResourceValue += resourceValue;
            }, cancellationToken: cancellationToken);
        }

        var gift = await _unitOfWork.GiftRepository.AddAsync(new Gift
        {
            ReceiverPlayerId = friendPlayerId,
            SenderPlayerId = senderPlayerId,
            ResourceValue = resourceValue,
            ResourceTypeId = existingResourceType.Id
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return gift;
    }
}