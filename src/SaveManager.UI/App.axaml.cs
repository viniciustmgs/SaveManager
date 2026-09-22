using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SaveManager.Application.DI;
using SaveManager.Infrastructure.DI;
using SaveManager.UI.DI;
using System;
using System.Threading.Tasks;

namespace SaveManager.UI
{
    public partial class App : Avalonia.Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("=== UNHANDLED EXCEPTION ===");
                System.Diagnostics.Debug.WriteLine(e.ExceptionObject?.ToString());
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("=== UNOBSERVED TASK EXCEPTION ===");
                System.Diagnostics.Debug.WriteLine(e.Exception.ToString());
                e.SetObserved();
            };

            var services = new ServiceCollection();

            services.AddInfrastructure();
            services.AddApplication();

            AppServiceProvider.Build(services);

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new Views.MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}