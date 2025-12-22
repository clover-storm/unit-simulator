namespace UnitSimulator.Core.Contracts;

/// <summary>
/// Minimal unit stats contract. Expanded in Phase 2 data pipeline.
/// </summary>
public sealed class UnitStats
{
    /// <summary>Hit points.</summary>
    public int HP { get; init; }
    /// <summary>Base damage per attack.</summary>
    public int Damage { get; init; }
    /// <summary>Attacks per second.</summary>
    public float AttackSpeed { get; init; }
    /// <summary>Movement speed.</summary>
    public float MoveSpeed { get; init; }
    /// <summary>Attack range distance.</summary>
    public float AttackRange { get; init; }
}
