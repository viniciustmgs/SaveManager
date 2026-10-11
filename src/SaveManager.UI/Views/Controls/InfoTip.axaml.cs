using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using Material.Icons;
using Material.Icons.Avalonia;

namespace SaveManager.UI.Views.Controls
{
    public sealed partial class InfoTip : Border
    {
        public static readonly StyledProperty<object> TipProperty =
            AvaloniaProperty.Register<InfoTip, object>(nameof(Tip));

        [Content]
        public object Tip
        {
            get => GetValue(TipProperty);
            set => SetValue(TipProperty, value);
        }

        public InfoTip()
        {
            InitializeComponent();

            Child = new MaterialIcon
            {
                Kind = MaterialIconKind.InformationOutline,
                Width = 14,
                Height = 14,
                Foreground = Brush.Parse("#9E9E9E")
            };

            Apply();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TipProperty)
                Apply();
        }

        private void Apply() => ToolTip.SetTip(this, Tip);
    }
}