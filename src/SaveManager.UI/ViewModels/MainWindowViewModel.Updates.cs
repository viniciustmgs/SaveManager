using CommunityToolkit.Mvvm.Input;
using SaveManager.Domain.Entities;
using SaveManager.Domain.Enums;
using SaveManager.Domain.Interfaces;
using SaveManager.UI.DI;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class MainWindowViewModel
    {
        private IReleaseChecker? _releaseChecker;
        private CancellationTokenSource? _updateCheckCts;

        private bool _isCheckUpdatesOpen;
        public bool IsCheckUpdatesOpen
        {
            get => _isCheckUpdatesOpen;
            set
            {
                if (SetProperty(ref _isCheckUpdatesOpen, value))
                {
                    if (!value)
                        CancelUpdateCheck();

                    NotifyOverlayStateChanged();
                }
            }
        }

        private bool _isUpdateCheckInProgress;
        public bool IsUpdateCheckInProgress
        {
            get => _isUpdateCheckInProgress;
            private set => SetProperty(ref _isUpdateCheckInProgress, value);
        }

        private UpdateCheckResult? _updateCheckResult;
        public UpdateCheckResult? UpdateCheckResult
        {
            get => _updateCheckResult;
            private set
            {
                if (SetProperty(ref _updateCheckResult, value))
                {
                    OnPropertyChanged(nameof(UpdateCheckHeadline));
                    OnPropertyChanged(nameof(UpdateCheckMessage));
                    OnPropertyChanged(nameof(IsUpdateAvailable));
                }
            }
        }

        public bool IsUpdateAvailable =>
            UpdateCheckResult?.Status == UpdateCheckStatus.UpdateAvailable;

        public string UpdateCheckHeadline => UpdateCheckResult switch
        {
            { Status: UpdateCheckStatus.UpToDate } => "You are up to date",
            { Status: UpdateCheckStatus.UpdateAvailable } => "Update available",
            { Status: UpdateCheckStatus.NoReleases } => "No releases found",
            { Status: UpdateCheckStatus.Failed } => "Could not check for updates",
            _ => string.Empty
        };

        public string UpdateCheckMessage => UpdateCheckResult switch
        {
            { Status: UpdateCheckStatus.UpToDate } =>
                $"{AppVersion} is the latest release.",
            { Status: UpdateCheckStatus.UpdateAvailable } =>
                $"You are running {AppVersion}. Version {UpdateCheckResult.LatestVersion} has been published.",
            { Status: UpdateCheckStatus.NoReleases } =>
                "There are no published releases on GitHub yet.",
            { Status: UpdateCheckStatus.Failed } =>
                "Something went wrong while contacting GitHub. Please try again.",
            _ => string.Empty
        };

        public string AppVersion { get; } = ReadAppVersion();

        private static string ReadAppVersion()
        {
            var assembly = Assembly.GetEntryAssembly();

            var informational = assembly
                ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

            if (string.IsNullOrWhiteSpace(informational))
                informational = assembly?.GetName().Version?.ToString();

            if (string.IsNullOrWhiteSpace(informational))
                return "v1.0.0";

            var plus = informational.IndexOf('+');

            if (plus > 0)
                informational = informational[..plus];

            return informational.StartsWith('v') ? informational : "v" + informational;
        }

        [RelayCommand]
        private async Task OpenCheckUpdates()
        {
            _releaseChecker ??= AppServiceProvider.GetService<IReleaseChecker>();

            UpdateCheckResult = null;
            IsUpdateCheckInProgress = true;
            IsCheckUpdatesOpen = true;

            var cts = new CancellationTokenSource();

            _updateCheckCts = cts;

            try
            {
                var result = await _releaseChecker.CheckAsync(AppVersion, cts.Token);

                if (!cts.IsCancellationRequested)
                    UpdateCheckResult = result;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (ReferenceEquals(_updateCheckCts, cts))
                {
                    IsUpdateCheckInProgress = false;

                    _updateCheckCts = null;
                }

                cts.Dispose();
            }
        }

        [RelayCommand]
        private void CloseCheckUpdates()
        {
            IsCheckUpdatesOpen = false;
        }

        [RelayCommand]
        private static void OpenReleasesPage(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch (Exception)
            {
                // no browser to hand off to
            }
        }

        private void CancelUpdateCheck()
        {
            var cts = _updateCheckCts;

            _updateCheckCts = null;
            IsUpdateCheckInProgress = false;

            cts?.Cancel();
        }
    }
}