using Game.BusinessLogicLayer.Services.Implementations;
using Game.BusinessLogicLayer.Services.Interfaces;
using Game.DataAccess.Context;
using Game.DataAccess.Repositories.Implementations;
using Game.DataAccess.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Game.BusinessLogicLayer;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterBllServices(this IServiceCollection services)
    {
        services.AddDbContext<GameDbContext>();
        services.AddScoped<IGameUnitOfWork, GameUnitOfWork>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IResourceTypeRepository, ResourceTypeRepository>();
        services.AddScoped<IGiftRepository, GiftRepository>();
        services.AddScoped<IPlayerService, PlayerService>();
        services.AddScoped<IGiftService, GiftService>();

        return services;
    }
}