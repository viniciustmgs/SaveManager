using Microsoft.Extensions.DependencyInjection;
using SaveManager.Application.UseCases.Game;
using SaveManager.Application.UseCases.Profile;
using SaveManager.Application.UseCases.Save;

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

            // use cases - save
            services.AddTransient<CreateSaveUseCase>();
            services.AddTransient<GetSavesUseCase>();
            services.AddTransient<LoadSaveUseCase>();
            services.AddTransient<ReplaceSaveUseCase>();
            services.AddTransient<DeleteSaveUseCase>();

            return services;
        }
    }
}