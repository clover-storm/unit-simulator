using UnitSimulator;

namespace UnitSimulator.Core.Contracts;

/// <summary>
/// Configuration for initializing a simulation instance.
/// </summary>
public sealed class GameConfig
{
    /// <summary>Map width used by the simulation.</summary>
    public int MapWidth { get; init; } = GameConstants.SIMULATION_WIDTH;

    /// <summary>Map height used by the simulation.</summary>
    public int MapHeight { get; init; } = GameConstants.SIMULATION_HEIGHT;

    /// <summary>Maximum number of frames to simulate.</summary>
    public int MaxFrames { get; init; } = GameConstants.MAX_FRAMES;

    /// <summary>Optional random seed for deterministic runs.</summary>
    public int? RandomSeed { get; init; }

    /// <summary>Initial wave number to start from.</summary>
    public int InitialWave { get; init; } = 0;

    /// <summary>Whether more waves are expected at initialization time.</summary>
    public bool HasMoreWaves { get; init; } = true;
}
