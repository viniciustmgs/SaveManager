using Microsoft.Extensions.DependencyInjection;
using SaveManager.Domain.Interfaces;
using SaveManager.Infrastructure.FileSystem;
using SaveManager.Infrastructure.Persistence;

namespace SaveManager.Infrastructure.DI
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSingleton<IGameRepository, JsonGameRepository>();
            services.AddSingleton<ISaveFileService, SaveFileService>();

            return services;
        }
    }
}