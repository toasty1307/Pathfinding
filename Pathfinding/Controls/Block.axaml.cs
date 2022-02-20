using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Pathfinding.Controls;

public class Block : UserControl
{
    public BlockViewModel ViewModel { get; set; }

    public event Action<Block>? BlockTypeChanged;

    public BlockType BlockType
    {
        get => ViewModel.BlockType;
        set
        {
            ViewModel.BlockType = value;
            Background = new SolidColorBrush(ViewModel.Color);
            BlockTypeChanged?.Invoke(this);
        }
    }


    public Block()
    {
        ViewModel = new BlockViewModel();

        void Handler(object? _, PointerEventArgs args)
        {
            BlockType = BlockType switch
            {
                BlockType.Obstacle when args.GetCurrentPoint(null).Properties.IsRightButtonPressed => BlockType.None,
                BlockType.None or BlockType.Path when args.GetCurrentPoint(null).Properties.IsLeftButtonPressed => BlockType.Obstacle,
                _ => BlockType
            };
        }
        
        AddHandler(PointerEnterEvent,    Handler, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent,    Handler, RoutingStrategies.Tunnel);
        AddHandler(PointerPressedEvent,  Handler, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, Handler, RoutingStrategies.Tunnel);
        
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}