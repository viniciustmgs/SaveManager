using Microsoft.Extensions.DependencyInjection;
using System;

namespace SaveManager.UI.DI
{
    public static class AppServiceProvider
    {
        private static ServiceProvider? _provider;

        public static void Build(IServiceCollection services)
        {
            _provider = services.BuildServiceProvider();
        }

        public static T GetService<T>() where T : notnull
        {
            if (_provider == null)
                throw new InvalidOperationException("ServiceProvider not built yet");

            return _provider.GetRequiredService<T>();
        }
    }
}