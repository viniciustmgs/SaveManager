using Microsoft.Extensions.DependencyInjection;
using SaveManager.Application.UseCases.Game;
using SaveManager.Application.UseCases.Profile;
using SaveManager.Application.UseCases.Save;
using SaveManager.Application.UseCases.Settings;

namespace SaveManager.Application.DI
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            // use cases - game
            services.AddTransient<AddGameUseCase>();
            services.AddTransient<GetGamesUseCase>();
            services.AddTransient<GetGameByIdUseCase>();
            services.AddTransient<UpdateGameUseCase>();
            services.AddTransient<RemoveGameUseCase>();

            // use cases - profile
            services.AddTransient<CreateProfileUseCase>();
            services.AddTransient<GetProfilesUseCase>();
            services.AddTransient<RemoveProfileUseCase>();
            services.AddTransient<RenameProfileUseCase>();

            // use cases - save
            services.AddTransient<CreateSaveUseCase>();
            services.AddTransient<GetSavesUseCase>();
            services.AddTransient<LoadSaveUseCase>();
            services.AddTransient<ReplaceSaveUseCase>();
            services.AddTransient<DeleteSaveUseCase>();
            services.AddTransient<RenameSaveUseCase>();

            // use cases - settings
            services.AddTransient<GetSettingsUseCase>();
            services.AddTransient<SaveSettingsUseCase>();

            return services;
        }
    }
}