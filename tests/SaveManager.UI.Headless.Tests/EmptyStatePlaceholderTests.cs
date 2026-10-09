using Avalonia.Controls;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using SaveManager.Application.DI;
using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Domain.Interfaces;
using SaveManager.Infrastructure.DI;
using SaveManager.Infrastructure.Persistence;
using SaveManager.Infrastructure.Persistence.Models;
using SaveManager.UI.ViewModels;
using System.Text.Json;

namespace SaveManager.UI.Headless.Tests
{
    public class EmptyStatePlaceholderTests : IDisposable
    {
        private readonly string _root =
            Path.Combine(Path.GetTempPath(), "sm-empty-" + Guid.NewGuid().ToString("N"));

        private string ConfigPath => Path.Combine(_root, "SaveManager", "config.json");
        private string BackupPath => Path.Combine(_root, "backup");

        public EmptyStatePlaceholderTests()
        {
            Directory.CreateDirectory(Path.Combine(_root, "SaveManager"));
            Directory.CreateDirectory(BackupPath);

            File.WriteAllText(
                ConfigPath,
                JsonSerializer.Serialize(
                    new AppConfig
                    {
                        Games = [],
                        Settings = new AppSettings { GlobalHotkeysEnabled = false }
                    }));
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_root, recursive: true);
            }
            catch (IOException)
            {
            }
        }

        [Fact]
        public void GamePickerShowsItsPlaceholderWhileThereAreNoGames()
        {
            var visible = Ui.Get(() =>
            {
                var (window, vm) = Open();
                vm.IsGamePickerOpen = true;
                return Find(window, "NoGamesPlaceholder").IsVisible;
            });

            Assert.True(visible);
        }

        [Fact]
        public void ProfilePickerShowsItsPlaceholderWhileThereAreNoProfiles()
        {
            var visible = Ui.Get(() =>
            {
                var (window, vm) = Open();
                vm.IsProfilePickerOpen = true;
                return Find(window, "NoProfilesPlaceholder").IsVisible;
            });

            Assert.True(visible);
        }

        [Fact]
        public void ThePlaceholderHidesAsSoonAsTheListHasSomethingInIt()
        {
            var visible = Ui.Get(() =>
            {
                var (window, vm) = Open();
                vm.IsGamePickerOpen = true;

                var text = Find(window, "NoGamesPlaceholder");

                vm.Games.Add(new Game { Name = "Dark Souls" });

                return text.IsVisible;
            });

            Assert.False(visible);
        }

        [Fact]
        public void ThePlaceholderComesBackWhenTheListEmptiesAgain()
        {
            var visible = Ui.Get(() =>
            {
                var (window, vm) = Open();
                vm.IsGamePickerOpen = true;

                var text = Find(window, "NoGamesPlaceholder");

                var game = new Game { Name = "Dark Souls" };
                vm.Games.Add(game);

                var hiddenWhileFull = text.IsVisible;

                vm.Games.Remove(game);

                return hiddenWhileFull || text.IsVisible;
            });

            Assert.True(visible);
        }

        [Fact]
        public void EditGamesShowsItsPlaceholderWhileThereAreNoGames()
        {
            var visible = Ui.Get(() =>
            {
                var (window, vm) = Open();
                vm.IsEditGamesDialogOpen = true;
                return Find(window, "NoGamesInEditPlaceholder").IsVisible;
            });

            Assert.True(visible);
        }

        [Fact]
        public void EditProfilesShowsItsPlaceholderWhileThereAreNoProfiles()
        {
            var visible = Ui.Get(() =>
            {
                var (window, vm) = Open();
                vm.IsEditProfilesDialogOpen = true;
                return Find(window, "NoProfilesInEditPlaceholder").IsVisible;
            });

            Assert.True(visible);
        }

        private static TextBlock Find(Window window, string name) =>
            window.GetVisualDescendants()
                .OfType<TextBlock>()
                .FirstOrDefault(t => t.Name == name)
                ?? throw new InvalidOperationException($"{name} was not found");

        private (Window, MainWindowViewModel) Open()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHotKeyWindowHost, NoWindowHost>();
            services.AddSingleton<IGlobalHotKeyService>(new NoopHotKeyService());
            services.AddSingleton(new AppConfigStore(ConfigPath));

            services.AddInfrastructure();
            services.AddApplication();

            UI.DI.AppServiceProvider.Build(services);

            var window = new Views.MainWindow();
            window.Show();
            window.Activate();

            return (window, (MainWindowViewModel)window.DataContext!);
        }

        private sealed class NoWindowHost : IHotKeyWindowHost
        {
            public bool IsAvailable => false;
            public IntPtr Handle => IntPtr.Zero;

            public IDisposable AddMessageHook(Func<IntPtr, uint, IntPtr, IntPtr, bool> handler) =>
                throw new NotSupportedException();
        }

        private sealed class NoopHotKeyService : IGlobalHotKeyService
        {
            public event Action<HotKeyAction>? HotKeyPressed
            {
                add { }
                remove { }
            }

            public bool IsSupported => false;
            public string? UnsupportedReason => null;

            public IReadOnlyDictionary<HotKeyAction, HotKeyRegistrationResult> Apply(
                IReadOnlyDictionary<HotKeyAction, HotKeyGesture> desired) =>
                new Dictionary<HotKeyAction, HotKeyRegistrationResult>();

            public void Dispose()
            {
            }
        }
    }
}
