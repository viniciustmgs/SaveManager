using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SaveManager.UI.ViewModels
{
    public partial class ToastViewModel : ObservableObject
    {
        private CancellationTokenSource? _cts;

        private string _message = string.Empty;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        private bool _isSuccess;
        public bool IsSuccess
        {
            get => _isSuccess;
            set => SetProperty(ref _isSuccess, value);
        }

        private bool _isWarning;
        public bool IsWarning
        {
            get => _isWarning;
            set => SetProperty(ref _isWarning, value);
        }

        private bool _isError;
        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }

        public async Task Show(string message, bool isSuccess, bool isWarning = false, int durationMs = 3000)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            Message = message;
            IsSuccess = isSuccess && !isWarning;
            IsWarning = isWarning;
            IsError = !isSuccess && !isWarning;
            IsVisible = true;

            try
            {
                await Task.Delay(durationMs, _cts.Token);
                IsVisible = false;
            }
            catch (TaskCanceledException)
            {
                // toast got replaced by a new one, don't do anything
            }
        }
    }
}