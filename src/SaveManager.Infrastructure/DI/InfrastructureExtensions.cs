using Microsoft.Extensions.DependencyInjection.Extensions;
﻿using Microsoft.Extensions.DependencyInjection;
using SaveManager.Domain.Interfaces;
using SaveManager.Infrastructure.FileSystem;
using SaveManager.Infrastructure.Persistence;
using SaveManager.Infrastructure.Updates;
using System;
using System.Net.Http;

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

            // TryAdd so tests can supply a fake
            services.TryAddSingleton<IReleaseChecker>(_ =>
                new GitHubReleaseChecker(new HttpClient { Timeout = TimeSpan.FromSeconds(10) }));

            // platform backend is picked once
            services.AddSingleton<IGlobalHotKeyService>(provider =>
                HotKeys.GlobalHotKeyServiceFactory.Create(
                    provider.GetRequiredService<IHotKeyWindowHost>()));

            return services;
        }
    }
}