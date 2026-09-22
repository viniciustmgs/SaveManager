using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Material.Icons;
using Material.Icons.Avalonia;

namespace SaveManager.UI.Views.Controls
{
    public partial class TitleBar : UserControl
    {
        public static readonly StyledProperty<bool> ShowMinimizeButtonProperty =
            AvaloniaProperty.Register<TitleBar, bool>(nameof(ShowMinimizeButton), true);

        public static readonly StyledProperty<bool> ShowMaximizeButtonProperty =
            AvaloniaProperty.Register<TitleBar, bool>(nameof(ShowMaximizeButton), true);

        public bool ShowMinimizeButton
        {
            get => GetValue(ShowMinimizeButtonProperty);
            set => SetValue(ShowMinimizeButtonProperty, value);
        }

        public bool ShowMaximizeButton
        {
            get => GetValue(ShowMaximizeButtonProperty);
            set => SetValue(ShowMaximizeButtonProperty, value);
        }

        private Window? _window;
        private MaterialIcon? _maximizeIcon;

        public TitleBar()
        {
            InitializeComponent();

            AttachedToVisualTree += OnAttachedToVisualTree;
            DetachedFromVisualTree += OnDetachedFromVisualTree;
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            _window = TopLevel.GetTopLevel(this) as Window;
            _maximizeIcon = this.FindControl<MaterialIcon>("MaximizeIcon");

            if (_window != null)
            {
                _window.PropertyChanged += OnWindowPropertyChanged;
                UpdateMaximizeIcon();
            }
        }

        private void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (_window != null)
                _window.PropertyChanged -= OnWindowPropertyChanged;

            _window = null;
        }

        private void OnWindowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Window.WindowStateProperty)
                UpdateMaximizeIcon();
        }

        private void UpdateMaximizeIcon()
        {
            if (_window == null || _maximizeIcon == null)
                return;

            _maximizeIcon.Kind = _window.WindowState == WindowState.Maximized
                ? MaterialIconKind.WindowRestore
                : MaterialIconKind.WindowMaximize;
        }

        private void OnDragArea_PointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (_window == null)
                return;

            var point = e.GetCurrentPoint(this);

            if (point.Properties.IsLeftButtonPressed)
            {
                e.Handled = true;
                _window.BeginMoveDrag(e);
            }
        }

        private void OnDragArea_DoubleTapped(object? sender, TappedEventArgs e)
        {
            if (_window == null || !ShowMaximizeButton)
                return;

            _window.WindowState = _window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void OnMinimizeClick(object? sender, RoutedEventArgs e)
        {
            if (_window != null)
                _window.WindowState = WindowState.Minimized;
        }

        private void OnMaximizeClick(object? sender, RoutedEventArgs e)
        {
            if (_window == null)
                return;

            _window.WindowState = _window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void OnCloseClick(object? sender, RoutedEventArgs e)
        {
            _window?.Close();
        }
    }
}