using Avalonia.Threading;
using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Domain.Interfaces;
using SaveManager.Infrastructure.HotKeys;
using System;
using System.Collections.Generic;

namespace SaveManager.UI.HotKeys
{
    // single owner of the global hotkey registrations
    public sealed class HotKeyCoordinator : IDisposable
    {
        private readonly IGlobalHotKeyService _service;
        private readonly HotKeyActivationState _activation = new();

        private AppSettings _settings = new();
        private bool _suspended;
        private bool _blocked;
        private bool _disposed;

        public event Action<HotKeyAction>? HotKeyPressed;

        public event Action<bool>? OthersEnabledChanged;

        public bool IsSupported => _service.IsSupported;

        public string? UnsupportedReason => _service.UnsupportedReason;

        public bool GlobalEnabled => _activation.GlobalEnabled;

        public bool OthersEnabled => _activation.OthersEnabled;

        public HotKeyCoordinator(IGlobalHotKeyService service)
        {
            _service = service;
            _service.HotKeyPressed += OnNativeHotKeyPressed;

            _activation.OthersEnabledChanged += enabled =>
                Dispatcher.UIThread.Post(() => OthersEnabledChanged?.Invoke(enabled));
        }

        public IReadOnlyDictionary<HotKeyAction, HotKeyRegistrationResult> Apply(AppSettings settings)
        {
            _settings = settings;
            _suspended = false;

            return Register(settings);
        }

        public void SetBlocked(bool value)
        {
            _blocked = value;

            (_service as IHotKeyInputBlocker)?.SetBlocked(value);
        }

        public void Suspend()
        {
            if (_disposed || _suspended)
                return;

            _suspended = true;

            _service.Apply(Unbind(BuildDesired(_settings)));
        }

        public void Resume()
        {
            if (_disposed || !_suspended)
                return;

            _suspended = false;

            Register(_settings);
        }

        private IReadOnlyDictionary<HotKeyAction, HotKeyRegistrationResult> Register(AppSettings settings)
        {
            var desired = BuildDesired(settings);

            _activation.SetGlobalEnabled(settings.GlobalHotkeysEnabled);

            if (!desired.ContainsKey(HotKeyAction.ToggleGlobalHotkeys))
                _activation.OnToggleBindingCleared();

            var toRegister = settings.GlobalHotkeysEnabled
                ? desired
                : Unbind(desired);

            return _service.Apply(toRegister);
        }

        private static Dictionary<HotKeyAction, HotKeyGesture> Unbind(
            Dictionary<HotKeyAction, HotKeyGesture> desired)
        {
            var empty = new Dictionary<HotKeyAction, HotKeyGesture>();

            foreach (var action in desired.Keys)
                empty[action] = HotKeyGesture.None;

            return empty;
        }

        public static Dictionary<HotKeyAction, HotKeyGesture> BuildDesired(AppSettings settings)
        {
            var desired = new Dictionary<HotKeyAction, HotKeyGesture>();

            foreach (var (action, value) in new[]
            {
                (HotKeyAction.CreateSave, settings.CreateSave),
                (HotKeyAction.LoadSave, settings.LoadSave),
                (HotKeyAction.NextSave, settings.NextSave),
                (HotKeyAction.PreviousSave, settings.PreviousSave),
                (HotKeyAction.ToggleGlobalHotkeys, settings.ToggleGlobalHotkeys)
            })
            {
                if (HotKeyGestureConverter.TryParse(value, out var gesture))
                    desired[action] = gesture;
            }

            return desired;
        }

        private void OnNativeHotKeyPressed(HotKeyAction action)
        {
            Dispatcher.UIThread.Post(() => Dispatch(action));
        }

        private void Dispatch(HotKeyAction action)
        {
            if (HotKeyDiagnostics.IsEnabled)
                HotKeyDiagnostics.Write(
                    $"gate {action}: blocked={_blocked} allowed={_activation.CanDispatch(action)}");

            if (_disposed || _blocked)
                return;

            if (!_activation.CanDispatch(action))
                return;

            if (action == HotKeyActivationState.ToggleAction)
            {
                _activation.Toggle();
                HotKeyPressed?.Invoke(action);
                return;
            }

            HotKeyPressed?.Invoke(action);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _service.HotKeyPressed -= OnNativeHotKeyPressed;
        }
    }
}