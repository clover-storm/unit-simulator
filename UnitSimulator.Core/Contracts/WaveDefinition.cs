namespace UnitSimulator.Core.Contracts;

/// <summary>
/// Minimal wave definition contract. Expanded in Phase 2 data pipeline.
/// </summary>
public sealed class WaveDefinition
{
    /// <summary>1-based wave index.</summary>
    public int WaveNumber { get; init; }
}
