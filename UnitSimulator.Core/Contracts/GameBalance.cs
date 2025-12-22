namespace UnitSimulator.Core.Contracts;

/// <summary>
/// Minimal game balance contract. Expanded in Phase 2 data pipeline.
/// </summary>
public sealed class GameBalance
{
    /// <summary>Contract version for balance data.</summary>
    public int Version { get; init; } = 1;
}
