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

        public async Task Show(string message, bool isSuccess, int durationMs = 3000)
        {
            // cancels any previous toast still visible
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            Message = message;
            IsSuccess = isSuccess;
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