using Avalonia;
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
    public class InfoTipTests : IDisposable
    {
        private const string HelpText = "The game only uses a single file to save everything.";

        private readonly string _root =
            Path.Combine(Path.GetTempPath(), "sm-infotip-" + Guid.NewGuid().ToString("N"));

        private string ConfigPath => Path.Combine(_root, "SaveManager", "config.json");

        public InfoTipTests()
        {
            Directory.CreateDirectory(Path.Combine(_root, "SaveManager"));

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
            try { Directory.Delete(_root, true); } catch { }
        }

        [Fact]
        public void SaveTypeLabelCarriesAnInfoTip()
        {
            var tip = Ui.Get(() =>
            {
                var window = OpenAddGameOverlay();

                return window.GetVisualDescendants()
                    .OfType<UI.Views.Controls.InfoTip>()
                    .FirstOrDefault();
            });

            Assert.NotNull(tip);
        }

        [Fact]
        public void InfoTipExposesItsContentAsATooltip()
        {
            var rendered = Ui.Get(() =>
            {
                var window = OpenAddGameOverlay();

                var tip = window.GetVisualDescendants()
                    .OfType<UI.Views.Controls.InfoTip>()
                    .FirstOrDefault();

                return ToolTip.GetTip(tip!) is StackPanel panel
                    ? panel.Children.OfType<TextBlock>().Select(t => t.Text).ToList()
                    : new List<string?>();
            });

            Assert.Contains("Single File:", rendered);
            Assert.Contains("Folder:", rendered);
        }

        [Fact]
        public void InfoTipIsHitTestable()
        {
            var (hasBackground, isHitTestVisible) = Ui.Get(() =>
            {
                var window = OpenAddGameOverlay();

                var tip = window.GetVisualDescendants()
                    .OfType<UI.Views.Controls.InfoTip>()
                    .FirstOrDefault()!;

                return (tip.Background is not null, tip.IsHitTestVisible);
            });

            Assert.True(hasBackground, "InfoTip has no background, so the pointer passes through it");
            Assert.True(isHitTestVisible);
        }

        [Fact]
        public void InfoTipRendersItsIcon()
        {
            var (iconCount, width) = Ui.Get(() =>
            {
                var window = OpenAddGameOverlay();

                var tip = window.GetVisualDescendants()
                    .OfType<UI.Views.Controls.InfoTip>()
                    .FirstOrDefault();

                tip!.Measure(new Size(200, 200));
                tip.Arrange(new Rect(tip.Bounds.Size));

                return (tip.GetVisualDescendants().OfType<Material.Icons.Avalonia.MaterialIcon>().Count(),
                        tip.Bounds.Width);
            });

            Assert.True(iconCount > 0, "no MaterialIcon inside the template");
            Assert.True(width > 0, "InfoTip has no width, so nothing can show");
        }

        [Fact]
        public void InfoTipDoesNotRenderItsContentInPlace()
        {
            var leaked = Ui.Get(() =>
            {
                var window = OpenAddGameOverlay();

                var tip = window.GetVisualDescendants()
                    .OfType<UI.Views.Controls.InfoTip>()
                    .FirstOrDefault();

                var expected = ToolTip.GetTip(tip!) is StackPanel panel
                    ? panel.Children.OfType<TextBlock>().Select(t => t.Text).ToList()
                    : new List<string?>();

                return window.GetVisualDescendants()
                    .OfType<TextBlock>()
                    .Select(t => t.Text)
                    .Intersect(expected)
                    .Any();
            });

            Assert.False(leaked);
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

        private Window OpenAddGameOverlay()
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

            var vm = (MainWindowViewModel)window.DataContext!;

            vm.IsAddGameDialogOpen = true;
            vm.IsAddGameDialogOpen = true;

            return window;
        }
    }
}