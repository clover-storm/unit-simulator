namespace UnitSimulator.Core.Contracts;

/// <summary>
/// Minimal unit stats contract. Expanded in Phase 2 data pipeline.
/// </summary>
public sealed class UnitStats
{
    public int HP { get; init; }
    public int Damage { get; init; }
    public float AttackSpeed { get; init; }
    public float MoveSpeed { get; init; }
    public float AttackRange { get; init; }
}
