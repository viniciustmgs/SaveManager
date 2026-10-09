using Microsoft.Extensions.DependencyInjection.Extensions;
﻿using Microsoft.Extensions.DependencyInjection;
using SaveManager.Domain.Interfaces;
using SaveManager.Infrastructure.FileSystem;
using SaveManager.Infrastructure.Persistence;

namespace SaveManager.Infrastructure.DI
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            // using TryAdd and not Add. a caller that already supplied a store (tests point this at a temp path) must win
            services.TryAddSingleton<AppConfigStore>();
            services.AddSingleton<IGameRepository, JsonGameRepository>();
            services.AddSingleton<ISaveFileService, SaveFileService>();
            services.AddSingleton<ISettingsRepository, JsonSettingsRepository>();

            // platform backend is picked once
            services.AddSingleton<IGlobalHotKeyService>(provider =>
                HotKeys.GlobalHotKeyServiceFactory.Create(
                    provider.GetRequiredService<IHotKeyWindowHost>()));

            return services;
        }
    }
}