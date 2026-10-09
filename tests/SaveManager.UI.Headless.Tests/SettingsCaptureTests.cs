using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using SaveManager.Application.DI;
using SaveManager.Domain.Interfaces;
using SaveManager.Infrastructure.DI;
using SaveManager.Infrastructure.Persistence;
using SaveManager.UI.ViewModels;
using SaveManager.UI.Views.Controls;
using System.Text.Json;

namespace SaveManager.UI.Headless.Tests
{
    public class SettingsCaptureTests : IDisposable
    {
        private readonly string _configPath;
        private readonly string _dir;

        public SettingsCaptureTests()
        {
            _dir = Path.Combine(Path.GetTempPath(), "sm-capture-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(_dir, "SaveManager"));

            _configPath = Path.Combine(_dir, "SaveManager", "config.json");

            WriteConfig(string.Empty);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_dir, recursive: true);
            }
            catch (IOException)
            {
            }
        }

        private void WriteConfig(string createSave) =>
            File.WriteAllText(_configPath, $$"""
            {
              "Games": [],
              "Settings": {
                "GlobalHotkeysEnabled": true,
                "CreateSave": "{{createSave}}",
                "LoadSave": "",
                "NextSave": "",
                "PreviousSave": "",
                "ToggleGlobalHotkeys": ""
              }
            }
            """);

        [Fact]
        public void ClickingTheBox_PromptsWithPressAKey()
        {
            var placeholder = Ui.Get(() =>
            {
                var (window, vm) = OpenSettings();

                var box = FirstBox(window);
                box.Focus();

                return box.PlaceholderText ?? string.Empty;
            });

            Assert.Equal("Press a Key", placeholder);
        }

        [Fact]
        public void LeavingTheBoxEmpty_ReturnsItToBlank()
        {
            var observed = Ui.Get(() =>
            {
                var (window, vm) = OpenSettings();

                var box = FirstBox(window);
                box.Focus();

                window.FocusManager!.Focus(null);

                return (box.Text ?? string.Empty, box.PlaceholderText ?? string.Empty);
            });

            Assert.Equal(string.Empty, observed.Item1);
            Assert.Equal(string.Empty, observed.Item2);
        }

        [Fact]
        public void ASingleKey_IsAccepted()
        {
            var boxText = Ui.Get(() =>
            {
                var (window, vm) = OpenSettings();

                var box = FirstBox(window);
                box.Focus();

                window.KeyPress(Key.F7, RawInputModifiers.None, PhysicalKey.F7, "F7");

                return box.Text ?? string.Empty;
            });

            Assert.Equal("F7", boxText);
        }

        [Fact]
        public void ACombination_IsAccepted()
        {
            var boxText = Ui.Get(() =>
            {
                var (window, vm) = OpenSettings();

                var box = FirstBox(window);
                box.Focus();

                window.KeyPress(
                    Key.S, RawInputModifiers.Control | RawInputModifiers.Shift, PhysicalKey.S, "S");

                return box.Text ?? string.Empty;
            });

            Assert.Equal("Ctrl+Shift+S", boxText);
        }

        [Fact]
        public void ClickingABoundBox_ClearsItAndPromptsAgain()
        {
            WriteConfig("NumPad7");

            var observed = Ui.Get(() =>
            {
                var (window, vm) = OpenSettings();

                var box = FirstBox(window);

                var before = box.Text ?? string.Empty;

                box.Focus();

                return (before, box.Text ?? string.Empty, box.PlaceholderText ?? string.Empty);
            });

            Assert.Equal("NumPad7", observed.Item1);
            Assert.Equal(string.Empty, observed.Item2);
            Assert.Equal("Press a Key", observed.Item3);
        }

        [Fact]
        public void ClickingABoundBox_ThenClickingAway_EmptiesTheSlot()
        {
            WriteConfig("NumPad7");

            var (boxText, placeholder, draft, unsaved) = Ui.Get(() =>
            {
                var (window, vm) = OpenSettings();

                var box = FirstBox(window);

                box.Focus();

                window.FocusManager!.Focus(null);

                return (box.Text ?? string.Empty, box.PlaceholderText ?? string.Empty,
                    vm.CreateSaveHotkey, vm.HasUnsavedChanges);
            });

            Assert.Equal(string.Empty, boxText);
            Assert.Equal(string.Empty, placeholder);
            Assert.Equal(string.Empty, draft);
            Assert.True(unsaved);
        }

        [Fact]
        public void ClearingASlot_PersistsWhenSaved()
        {
            WriteConfig("NumPad7");

            Ui.Run(async () =>
            {
                var (window, vm) = OpenSettings();

                FirstBox(window).Focus();
                window.FocusManager!.Focus(null);

                await vm.SaveSettingsCommand.ExecuteAsync(null);
            });

            Assert.Equal(string.Empty, ReadCreateSave());
        }

        [Fact]
        public void RepressingTheSameKeyAfterClearing_Works()
        {
            WriteConfig("F7");

            var observed = Ui.Get(() =>
            {
                var (window, vm) = OpenSettings();

                var box = FirstBox(window);
                box.Focus();

                var afterClick = box.Text ?? string.Empty;

                window.KeyPress(Key.F7, RawInputModifiers.None, PhysicalKey.F7, "F7");

                return (afterClick, box.Text ?? string.Empty, vm.CreateSaveHotkey);
            });

            Assert.Equal(string.Empty, observed.Item1);
            Assert.Equal("F7", observed.Item2);
            Assert.Equal("F7", observed.Item3);
        }

        [Fact]
        public void PressingAKeyOwnedByAnotherSlot_MovesItAcross()
        {
            WriteConfig("");

            File.WriteAllText(_configPath, """
            {
              "Games": [],
              "Settings": {
                "GlobalHotkeysEnabled": true,
                "CreateSave": "",
                "LoadSave": "Ctrl+Shift+S",
                "NextSave": "",
                "PreviousSave": "",
                "ToggleGlobalHotkeys": ""
              }
            }
            """);

            var observed = Ui.Get(() =>
            {
                var (window, vm) = OpenSettings();

                var boxes = AllBoxes(window);

                boxes[0].Focus();
                window.KeyPress(
                    Key.S, RawInputModifiers.Control | RawInputModifiers.Shift, PhysicalKey.S, "S");

                return (
                    boxes[0].Text ?? string.Empty,
                    boxes[1].Text ?? string.Empty,
                    vm.CreateSaveHotkey,
                    vm.LoadSaveHotkey);
            });

            Assert.Equal("Ctrl+Shift+S", observed.Item1);
            Assert.Equal(string.Empty, observed.Item2);
            Assert.Equal("Ctrl+Shift+S", observed.Item3);
            Assert.Equal(string.Empty, observed.Item4);
        }

        [Fact]
        public void ALayoutDependentKey_IsRefusedButStaysFocused()
        {
            var observed = Ui.Get(() =>
            {
                var (window, vm) = OpenSettings();

                var box = FirstBox(window);
                box.Focus();

                window.KeyPress(Key.OemComma, RawInputModifiers.Control, PhysicalKey.Comma, ",");

                return (
                    box.PlaceholderText ?? string.Empty,
                    ReferenceEquals(box, window.FocusManager?.GetFocusedElement()));
            });

            Assert.Equal(HotkeyTextBox.UnsupportedPlaceholder, observed.Item1);
            Assert.True(observed.Item2);
        }

        [Fact]
        public void CapturedSingleKey_ReachesTheConfigFile()
        {
            Ui.Run(async () =>
            {
                var (window, vm) = OpenSettings();

                var box = FirstBox(window);
                box.Focus();

                window.KeyPress(Key.F7, RawInputModifiers.None, PhysicalKey.F7, "F7");

                await vm.SaveSettingsCommand.ExecuteAsync(null);
            });

            Assert.Equal("F7", ReadCreateSave());
        }

        [Fact]
        public void CapturedCombination_ReachesTheConfigFile()
        {
            Ui.Run(async () =>
            {
                var (window, vm) = OpenSettings();

                var box = FirstBox(window);
                box.Focus();

                window.KeyPress(Key.S, RawInputModifiers.Control, PhysicalKey.S, "s");

                await vm.SaveSettingsCommand.ExecuteAsync(null);
            });

            Assert.Equal("Ctrl+S", ReadCreateSave());
        }

        [Fact]
        public void AnExistingSingleKeyBinding_IsShownOnOpen()
        {
            WriteConfig("NumPad7");

            var (text, unsaved) = Ui.Get(() =>
            {
                var (window, vm) = OpenSettings();

                return (FirstBox(window).Text ?? string.Empty, vm.HasUnsavedChanges);
            });

            Assert.Equal("NumPad7", text);
            Assert.False(unsaved);
        }

        [Fact]
        public void AcceptedKey_ReleasesFocus()
        {
            var stillFocused = Ui.Get(() =>
            {
                var (window, vm) = OpenSettings();

                var box = FirstBox(window);
                box.Focus();

                window.KeyPress(Key.F7, RawInputModifiers.None, PhysicalKey.F7, "F7");

                return ReferenceEquals(box, window.FocusManager?.GetFocusedElement());
            });

            Assert.False(stillFocused);
        }

        private string ReadCreateSave()
        {
            using var config = JsonDocument.Parse(File.ReadAllText(_configPath));

            return config.RootElement
                .GetProperty("Settings")
                .GetProperty("CreateSave")
                .GetString() ?? string.Empty;
        }

        private void SetupServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IHotKeyWindowHost, NoWindowHost>();
            services.AddSingleton(new AppConfigStore(_configPath));

            services.AddInfrastructure();
            services.AddApplication();

            UI.DI.AppServiceProvider.Build(services);
        }

        private (Window, MainWindowViewModel) OpenSettings()
        {
            SetupServices();

            var window = new Views.MainWindow();
            window.Show();
            window.Activate();

            var vm = (MainWindowViewModel)window.DataContext!;
            vm.OpenSettingsCommand.Execute(null);

            return (window, vm);
        }

        private static HotkeyTextBox FirstBox(Window window) => AllBoxes(window)[0];

        private static List<HotkeyTextBox> AllBoxes(Window window)
        {
            var boxes = new List<HotkeyTextBox>();

            void Walk(Visual visual)
            {
                if (visual is HotkeyTextBox box)
                    boxes.Add(box);

                foreach (var child in visual.GetVisualChildren())
                    Walk(child);
            }

            Walk((Visual)window.Content!);

            if (boxes.Count != 5)
                throw new InvalidOperationException($"expected 5 capture boxes, found {boxes.Count}");

            return boxes;
        }

        private sealed class NoWindowHost : IHotKeyWindowHost
        {
            public bool IsAvailable => false;
            public IntPtr Handle => IntPtr.Zero;

            public IDisposable AddMessageHook(Func<IntPtr, uint, IntPtr, IntPtr, bool> handler) =>
                new Nothing();
        }

        private sealed class Nothing : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}