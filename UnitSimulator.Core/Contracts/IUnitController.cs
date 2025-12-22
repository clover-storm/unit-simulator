using System.Numerics;
using UnitSimulator;

namespace UnitSimulator.Core.Contracts;

/// <summary>
/// High-level unit control operations for external systems.
/// </summary>
public interface IUnitController
{
    /// <summary>Queues a unit spawn.</summary>
    void SpawnUnit(Vector2 position, UnitRole role, UnitFaction faction, int? hp = null, float? speed = null, float? turnSpeed = null);

    /// <summary>Queues a unit move command.</summary>
    void MoveUnit(int unitId, UnitFaction faction, Vector2 destination);

    /// <summary>Queues a unit damage command.</summary>
    void DamageUnit(int unitId, UnitFaction faction, int damage);

    /// <summary>Queues a unit kill command.</summary>
    void KillUnit(int unitId, UnitFaction faction);

    /// <summary>Queues a unit revive command.</summary>
    void ReviveUnit(int unitId, UnitFaction faction, int hp);

    /// <summary>Queues a unit health set command.</summary>
    void SetUnitHealth(int unitId, UnitFaction faction, int hp);

    /// <summary>Queues a unit removal command.</summary>
    void RemoveUnit(int unitId, UnitFaction faction);
}
