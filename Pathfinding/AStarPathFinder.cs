using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;

namespace Pathfinding;

public class Node
{
    public Vector2 Position;
    public float DistanceToTarget;
    public float Cost;
    
    [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
    public float F
    {
        get
        {
            if (DistanceToTarget != -1 && Cost != -1)
                return DistanceToTarget + Cost;
            return -1;
        }
    }    
    public bool Walkable;
    public Node Parent = null!;

    public Node(Vector2 position, bool walkable)
    {
        Position = position;
        Walkable = walkable;
        DistanceToTarget = -1;
        Cost = 1;
    }
}

public class AStarPathFinder
{
    public Vector2[] FindPath(int startX, int startY, int targetX, int targetY, Vector2[] obstacles)
    {
        var start = new Node(new Vector2(startX, startY), true);
        var target = new Node(new Vector2(targetX, targetY), true);

        var path = new Stack<Node>();
        var openList = new List<Node>();
        var closedList = new List<Node>();
        var obstacleNodes = obstacles.Select(x => new Node(x, false)).ToList();
        var current = start;

        openList.Add(start);

        var dist = Math.Abs(start.Position.X - target.Position.X) +
                   Math.Abs(start.Position.Y - target.Position.Y);
        
        var iterations = 0;
        while (openList.Any() && closedList.All(x => x.Position != target.Position))
        {
            iterations++;
            if (iterations > dist * dist * dist) // random number i chose idk what would be a good thing to use
                return null!;
            current = openList.First();
            openList.Remove(current);
            closedList.Add(current);
            var neighbours = GetNeighbours(current, obstacleNodes.ToArray());

            foreach (var neighbour in 
                     neighbours
                         .Where(neighbour =>
                             !closedList.Contains(neighbour))
                         .Where(neighbour => 
                             !openList.Contains(neighbour)))
            {
                neighbour.Parent = current;
                neighbour.DistanceToTarget = Math.Abs(neighbour.Position.X - target.Position.X) + Math.Abs(neighbour.Position.Y - target.Position.Y);
                neighbour.Cost = current.Cost + 1;
                openList.Add(neighbour);
                openList = openList.OrderBy(x => x.F).ToList();
            }
        }

        if (closedList.All(x => x.Position != target.Position))
            return null!;

        var temp = closedList[closedList.IndexOf(current)];
        if (temp == null!)
            return null!;

        do
        {
            path.Push(temp);
            temp = temp.Parent;
        } while (temp != start && temp != null!);

        return path.Select(x => x.Position).ToArray();
    }

    private List<Node> GetNeighbours(Node current, Node[] obstacles)
    {
        var neighbours = new[]
        {
            new[] { 1,  0}, // right
            new[] {-1,  0}, // left
            new[] { 0,  1}, // up
            new[] { 0, -1}, // down
            new[] { 1,  1}, // right up
            new[] { 1, -1}, // right down
            new[] {-1,  1}, // left up 
            new[] {-1, -1}, // left down
        };

        var list = new List<Node>();
        foreach (var neighbour in neighbours)
        {
            var x = (int) current.Position.X + neighbour[0];
            var y = (int) current.Position.Y + neighbour[1];
            if (x < 0 || y < 0)
                continue;
            var node = new Node(new Vector2(x, y), true);
            if (obstacles.Any(n => n.Position == node.Position))
                continue;

            list.Add(node);
        }

        return list;
    }
}