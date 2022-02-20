using System;
using Avalonia.Media;
using Pathfinding.ViewModels;
using ReactiveUI;

namespace Pathfinding.Controls;

public enum BlockType
{
    Start,
    End,
    Obstacle,
    Path,
    None,
}

public class BlockViewModel : ViewModelBase
{
    private BlockType _blockType = BlockType.None;
    
    public BlockType BlockType
    {
        get => _blockType;
        set => this.RaiseAndSetIfChanged(ref _blockType, value);
    }

    public Color Color
    {
        get
        {
            return BlockType switch
            {
                BlockType.Start => Color.Parse("#198754"),
                BlockType.End => Color.Parse("#0DCAF0"),
                BlockType.Obstacle => Color.Parse("#DC3545"),
                BlockType.Path => Color.Parse("#0D6EFD"),
                BlockType.None => Color.Parse("#282C34"),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}