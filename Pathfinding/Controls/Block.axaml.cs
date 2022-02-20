using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Pathfinding.Controls;

public partial class BLock : UserControl
{
    public BLock()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}