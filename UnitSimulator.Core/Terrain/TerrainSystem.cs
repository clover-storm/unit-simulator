using System.Numerics;

namespace UnitSimulator;

public class TerrainSystem
{
    private static readonly Vector2 LeftBridgeCenter = new(
        (MapLayout.LeftBridgeXMin + MapLayout.LeftBridgeXMax) / 2f,
        (MapLayout.RiverYMin + MapLayout.RiverYMax) / 2f);

    private static readonly Vector2 RightBridgeCenter = new(
        (MapLayout.RightBridgeXMin + MapLayout.RightBridgeXMax) / 2f,
        (MapLayout.RiverYMin + MapLayout.RiverYMax) / 2f);

    public bool CanMoveTo(Unit unit, Vector2 position)
    {
        if (unit.Layer == MovementLayer.Air)
        {
            return MapLayout.IsWithinBounds(position);
        }

        return MapLayout.IsWithinBounds(position) && MapLayout.CanGroundUnitMoveTo(position);
    }

    public Vector2 GetAdjustedDestination(Unit unit, Vector2 destination)
    {
        if (unit.Layer == MovementLayer.Air)
        {
            return MapLayout.ClampToBounds(destination);
        }

        if (!IsCrossingRiver(unit.Position, destination))
        {
            return MapLayout.ClampToBounds(destination);
        }

        if (MapLayout.IsOnBridge(unit.Position) || MapLayout.IsOnBridge(destination))
        {
            return MapLayout.ClampToBounds(destination);
        }

        return GetNearestBridgeCenter(unit.Position);
    }

    private static bool IsCrossingRiver(Vector2 from, Vector2 to)
    {
        bool fromLower = from.Y < MapLayout.RiverYMin;
        bool fromUpper = from.Y > MapLayout.RiverYMax;
        bool toLower = to.Y < MapLayout.RiverYMin;
        bool toUpper = to.Y > MapLayout.RiverYMax;

        return (fromLower && toUpper) || (fromUpper && toLower);
    }

    private static Vector2 GetNearestBridgeCenter(Vector2 position)
    {
        float leftDistance = Vector2.Distance(position, LeftBridgeCenter);
        float rightDistance = Vector2.Distance(position, RightBridgeCenter);
        return leftDistance <= rightDistance ? LeftBridgeCenter : RightBridgeCenter;
    }
}
