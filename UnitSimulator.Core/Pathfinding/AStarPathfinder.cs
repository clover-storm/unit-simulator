using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace UnitSimulator.Core.Pathfinding
{
    public class AStarPathfinder
    {
        private readonly PathfindingGrid _grid;

        public AStarPathfinder(PathfindingGrid grid)
        {
            _grid = grid;
        }

        public List<Vector2>? FindPath(Vector2 startWorldPos, Vector2 endWorldPos)
        {
            PathNode? startNode = _grid.NodeFromWorldPoint(startWorldPos);
            PathNode? endNode = _grid.NodeFromWorldPoint(endWorldPos);

            if (startNode == null || endNode == null || !startNode.IsWalkable || !endNode.IsWalkable)
            {
                return null;
            }

            var openList = new List<PathNode> { startNode };
            var closedList = new HashSet<PathNode>();

            _grid.ResetAllNodes();

            startNode.GCost = 0;
            startNode.HCost = CalculateDistanceCost(startNode, endNode);

            while (openList.Count > 0)
            {
                PathNode currentNode = openList[0];
                for (int i = 1; i < openList.Count; i++)
                {
                    if (openList[i].FCost < currentNode.FCost || openList[i].FCost == currentNode.FCost && openList[i].HCost < currentNode.HCost)
                    {
                        currentNode = openList[i];
                    }
                }
                
                if (currentNode == endNode)
                {
                    return RetracePath(startNode, endNode);
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                foreach (PathNode neighborNode in GetNeighbors(currentNode))
                {
                    if (!neighborNode.IsWalkable || closedList.Contains(neighborNode))
                    {
                        continue;
                    }

                    int tentativeGCost = currentNode.GCost + CalculateDistanceCost(currentNode, neighborNode);
                    if (tentativeGCost < neighborNode.GCost)
                    {
                        neighborNode.CameFromNode = currentNode;
                        neighborNode.GCost = tentativeGCost;
                        neighborNode.HCost = CalculateDistanceCost(neighborNode, endNode);

                        if (!openList.Contains(neighborNode))
                        {
                            openList.Add(neighborNode);
                        }
                    }
                }
            }

            // No path found
            return null;
        }

        private List<Vector2> RetracePath(PathNode startNode, PathNode endNode)
        {
            var path = new List<PathNode>();
            PathNode? currentNode = endNode;
            while (currentNode != null && currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.CameFromNode;
            }
            path.Reverse();
            return path.Select(p => p.WorldPosition).ToList();
        }

        private int CalculateDistanceCost(PathNode a, PathNode b)
        {
            int xDistance = Math.Abs(a.X - b.X);
            int yDistance = Math.Abs(a.Y - b.Y);
            int remaining = Math.Abs(xDistance - yDistance);
            // Cost for diagonal move is 14, cost for straight move is 10
            return 14 * Math.Min(xDistance, yDistance) + 10 * remaining;
        }

        private IEnumerable<PathNode> GetNeighbors(PathNode node)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    int nx = node.X + x;
                    int ny = node.Y + y;
                    PathNode? neighbor = _grid.GetNode(nx, ny);
                    if (neighbor != null)
                    {
                        if (x != 0 && y != 0)
                        {
                            var horizontal = _grid.GetNode(node.X + x, node.Y);
                            var vertical = _grid.GetNode(node.X, node.Y + y);
                            if (horizontal != null && vertical != null &&
                                (!horizontal.IsWalkable || !vertical.IsWalkable))
                            {
                                continue;
                            }
                        }
                        yield return neighbor;
                    }
                }
            }
        }
    }
}
