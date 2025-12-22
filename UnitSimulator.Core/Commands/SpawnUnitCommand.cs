using System.Numerics;

namespace UnitSimulator;

/// <summary>
/// Command to spawn a new unit in the simulation.
/// </summary>
public record SpawnUnitCommand(
    int FrameNumber,
    Vector2 Position,
    UnitRole Role,
    UnitFaction Faction,
    int? HP = null,
    float? Speed = null,
    float? TurnSpeed = null
) : ISimulationCommand;

/// <summary>
/// Command to move a unit to a new position.
/// </summary>
public record MoveUnitCommand(
    int FrameNumber,
    int UnitId,
    UnitFaction Faction,
    Vector2 Destination
) : ISimulationCommand;

/// <summary>
/// Command to deal damage to a unit.
/// </summary>
public record DamageUnitCommand(
    int FrameNumber,
    int UnitId,
    UnitFaction Faction,
    int Damage
) : ISimulationCommand;

/// <summary>
/// Command to kill a unit immediately.
/// </summary>
public record KillUnitCommand(
    int FrameNumber,
    int UnitId,
    UnitFaction Faction
) : ISimulationCommand;

/// <summary>
/// Command to revive a dead unit.
/// </summary>
public record ReviveUnitCommand(
    int FrameNumber,
    int UnitId,
    UnitFaction Faction,
    int HP
) : ISimulationCommand;

/// <summary>
/// Command to remove a unit from the simulation entirely.
/// </summary>
public record RemoveUnitCommand(
    int FrameNumber,
    int UnitId,
    UnitFaction Faction
) : ISimulationCommand;
