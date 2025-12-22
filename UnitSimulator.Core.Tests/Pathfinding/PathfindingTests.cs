using System.Numerics;
using UnitSimulator.Core.Pathfinding;
using Xunit;

namespace UnitSimulator.Core.Tests.Pathfinding;

public class PathfindingTests
{
    [Fact]
    public void FindPath_ReturnsNull_WhenStartOrEndBlocked()
    {
        var grid = new PathfindingGrid(100, 100, 10);
        var pathfinder = new AStarPathfinder(grid);

        grid.SetWalkable(0, 0, false);
        var pathFromBlockedStart = pathfinder.FindPath(new Vector2(1, 1), new Vector2(50, 50));
        Assert.Null(pathFromBlockedStart);

        grid.SetWalkable(0, 0, true);
        grid.SetWalkable(5, 5, false);
        var pathToBlockedEnd = pathfinder.FindPath(new Vector2(1, 1), new Vector2(55, 55));
        Assert.Null(pathToBlockedEnd);
    }

    [Fact]
    public void FindPath_AvoidsBlockedNodes()
    {
        var grid = new PathfindingGrid(100, 100, 10);
        var pathfinder = new AStarPathfinder(grid);

        grid.SetWalkable(1, 1, false);

        var path = pathfinder.FindPath(new Vector2(5, 5), new Vector2(35, 35));
        Assert.NotNull(path);
        foreach (var waypoint in path!)
        {
            var node = grid.NodeFromWorldPoint(waypoint);
            Assert.NotNull(node);
            Assert.True(node!.IsWalkable);
        }
    }

    [Fact]
    public void FindPath_DoesNotCutCorners()
    {
        var grid = new PathfindingGrid(20, 20, 10);
        var pathfinder = new AStarPathfinder(grid);

        grid.SetWalkable(1, 0, false);
        grid.SetWalkable(0, 1, false);

        var path = pathfinder.FindPath(new Vector2(5, 5), new Vector2(15, 15));
        Assert.Null(path);
    }
}
