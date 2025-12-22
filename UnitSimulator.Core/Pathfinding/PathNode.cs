using System.Numerics;

namespace UnitSimulator.Core.Pathfinding
{
    public class PathNode
    {
        public int X { get; }
        public int Y { get; }
        public Vector2 WorldPosition { get; }
        public bool IsWalkable { get; set; }

        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost => GCost + HCost;

        public PathNode? CameFromNode { get; set; }

        public PathNode(int x, int y, Vector2 worldPosition, bool isWalkable = true)
        {
            X = x;
            Y = y;
            WorldPosition = worldPosition;
            IsWalkable = isWalkable;
            ResetCosts();
        }

        public void ResetCosts()
        {
            GCost = int.MaxValue;
            HCost = 0;
            CameFromNode = null;
        }
    }
}
