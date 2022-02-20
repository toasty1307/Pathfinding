using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Pathfinding.Controls;
using Serilog;

namespace Pathfinding.Views;

public class MainWindow : Window
{
    // ReSharper disable once MemberInitializerValueIgnored
    private Block[,] _allBlocks = null!;
    private TextBlock _statusTextBlock;
    private readonly List<Block> _blockPool;
    private readonly NumericUpDown _blocksSizeX;
    private readonly NumericUpDown _blocksSizeY;
    private int MaxBlocksX => (int) _blocksSizeX.Maximum;
    private int MaxBlocksY => (int) _blocksSizeY.Maximum;
    private int NumBlocksX => (int) _blocksSizeX.Value;
    private int NumBlocksY => (int) _blocksSizeY.Value;
    private readonly AStarPathFinder _finder = new();
    private readonly Panel _panel;

    private const int BlockMargin = 5;

    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        _statusTextBlock = this.FindControl<TextBlock>("StatusTextBlock");
        _panel = this.Find<Panel>("Blocks");
        _blocksSizeX = this.Find<NumericUpDown>("BlocksSizeX");
        _blocksSizeY = this.Find<NumericUpDown>("BlocksSizeY");
        _blockPool = Enumerable.Range(0, MaxBlocksX * MaxBlocksY).Select(_ =>
        {
            var block = new Block
            {
                IsVisible = false,
                BlockType = BlockType.None
            };
            block.BlockTypeChanged += b =>
            {
                if (_allBlocks[NumBlocksX - 1, NumBlocksY - 1] == null!) return;
                
                var obstacles = new List<Vector2>();

                for (var x = 0; x < NumBlocksX; x++) 
                for (var y = 0; y < NumBlocksY; y++) 
                    if (_allBlocks[x, y].BlockType == BlockType.Obstacle)
                        obstacles.Add(new Vector2(x, y));

                var path = _finder.FindPath(0, 0, 
                    NumBlocksX - 1, NumBlocksY - 1,
                    obstacles.ToArray()
                );
                HighlightPath(path);
            };
            return block;
        }).ToList();
        MakeBlocks();
    }

    private void MakeBlocks()
    {
        _allBlocks = new Block[NumBlocksX, NumBlocksY];
        _panel.Children.Clear();
        _blockPool.ForEach(x => x.IsVisible = false);
        var width = (_panel.Width - NumBlocksX * BlockMargin) / NumBlocksX;
        var height = (_panel.Height - NumBlocksY * BlockMargin) / NumBlocksY;
        for (var x = 0; x < NumBlocksX; x++)
        {
            var leftMargin = x * width;
            for (var y = 0; y < NumBlocksY; y++)
            {
                var block = _blockPool.FirstOrDefault(b => b.IsVisible == false) ??
                            throw new Exception("No blocks left");
                block.Width = width;
                block.Height = height;
                block.BlockType = BlockType.None;
                block.Margin = new Thickness(leftMargin + BlockMargin * x, y * (height + BlockMargin), 0, 0);
                block.HorizontalAlignment = HorizontalAlignment.Left;
                block.VerticalAlignment = VerticalAlignment.Top;
                block.IsVisible = true;
                _allBlocks[x, y] = block;
                _panel.Children.Add(block);
            }
        }
        SetBlockTypeWithoutEvent(_allBlocks[0, 0], BlockType.Start);
        SetBlockTypeWithoutEvent(_allBlocks[NumBlocksX - 1, NumBlocksY - 1], BlockType.End);
        _allBlocks[0, 0].Margin = new Thickness(0);
        var path = _finder.FindPath(0, 0, NumBlocksX - 1, NumBlocksY - 1, Array.Empty<Vector2>());
        HighlightPath(path);
    }

    private void HighlightPath(IEnumerable<Vector2> path)
    {
        _statusTextBlock.Text = string.Empty;
        if (path == null!)
        {
            _statusTextBlock.Text = "No Path!";
            _statusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
            ResetBlocks(false);
            return;
        }
        ResetBlocks(false);
        foreach (var vector2 in path)
        {
            var x = (int) vector2.X;
            var y = (int) vector2.Y;
            if (x >= NumBlocksX || y >= NumBlocksY)
            {
                _statusTextBlock.Text = "No Path!";
                _statusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
                ResetBlocks(false);
                return;
            }
            var block = _allBlocks[x, y];
            if (block.BlockType == BlockType.None)
                SetBlockTypeWithoutEvent(block, BlockType.Path);
        }
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void ResetButtonClicked(object? sender, RoutedEventArgs e)
    {
        ResetBlocks();
        Log.Information("Clear button clicked");
        var path = _finder.FindPath(0, 0, NumBlocksX - 1, NumBlocksY - 1, Array.Empty<Vector2>());
        HighlightPath(path);
    }

    private void ResetBlocks(bool resetObstacles = true)
    {
        for (var x = 0; x < NumBlocksX; x++)
        for (var y = 0; y < NumBlocksY; y++) 
            switch (resetObstacles)
            {
                case true:
                case false when _allBlocks[x, y].BlockType != BlockType.Obstacle:
                    SetBlockTypeWithoutEvent(_allBlocks[x, y], BlockType.None);
                    break;
            }
        
        SetBlockTypeWithoutEvent(_allBlocks[0, 0], BlockType.Start);
        SetBlockTypeWithoutEvent(_allBlocks[NumBlocksX - 1, NumBlocksY - 1], BlockType.End);
    }

    private void SetBlockTypeWithoutEvent(Block block, BlockType type)
    {
        block.ViewModel.BlockType = type;
        block.Background = new SolidColorBrush(block.ViewModel.Color);
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void HeightValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        Log.Information("Height changed: {Height}", e.NewValue);
        MakeBlocks();
    }

    [SuppressMessage("ReSharper", "UnusedParameter.Local")]
    private void WidthValueChanged(object? sender, NumericUpDownValueChangedEventArgs e)
    {
        Log.Information("Width changed: {Width}", e.NewValue);
        MakeBlocks();
    }
}