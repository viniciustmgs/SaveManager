using Avalonia;
using Avalonia.Controls;
using SaveManager.UI.ViewModels;

namespace SaveManager.UI.Views
{
    public partial class ToastView : UserControl
    {
        public static readonly StyledProperty<ToastViewModel?> ToastProperty =
            AvaloniaProperty.Register<ToastView, ToastViewModel?>(nameof(Toast));

        public ToastViewModel? Toast
        {
            get => GetValue(ToastProperty);
            set => SetValue(ToastProperty, value);
        }

        public ToastView()
        {
            InitializeComponent();
        }
    }
}