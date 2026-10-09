using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using SaveManager.Application.DI;
using SaveManager.Domain.Interfaces;
using SaveManager.Infrastructure.DI;
using SaveManager.UI.DI;
using SaveManager.UI.HotKeys;
using System;
using System.Threading.Tasks;

namespace SaveManager.UI
{
    public partial class App : Avalonia.Application
    {
        private readonly WindowHostAccessor _windowHost = new();

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

            services.AddSingleton(_windowHost);
            services.AddSingleton<IHotKeyWindowHost>(provider =>
                new AvaloniaHotKeyWindowHost(() => provider.GetRequiredService<WindowHostAccessor>().Window));

            services.AddInfrastructure();
            services.AddApplication();

            AppServiceProvider.Build(services);

            _ = AppServiceProvider.GetService<IGlobalHotKeyService>();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var window = new Views.MainWindow();

                _windowHost.Window = window;

                // the Win32 backend needs a real HWND
                window.Opened += OnMainWindowOpened;

                desktop.MainWindow = window;
                desktop.Exit += OnDesktopExit;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void OnMainWindowOpened(object? sender, EventArgs e)
        {
            if (sender is not Window window)
                return;

            if (window.DataContext is not ViewModels.MainWindowViewModel vm)
                return;

            var coordinator = new HotKeyCoordinator(AppServiceProvider.GetService<IGlobalHotKeyService>());

            vm.InitializeHotKeys(coordinator);

            if (OperatingSystem.IsWindows()
                && !coordinator.IsSupported
                && coordinator.UnsupportedReason is { } reason)
            {
                _ = vm.Toast.Show(reason, isSuccess: false);
            }
        }

        private void OnDesktopExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            // release the OS hotkeys before the process ends
            AppServiceProvider.GetService<IGlobalHotKeyService>()?.Dispose();
        }
    }
}