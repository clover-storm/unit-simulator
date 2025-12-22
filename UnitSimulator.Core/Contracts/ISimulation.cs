using UnitSimulator;

namespace UnitSimulator.Core.Contracts;

/// <summary>
/// Main simulation contract for engine adapters.
/// </summary>
public interface ISimulation
{
    /// <summary>Initializes the simulation with the provided configuration.</summary>
    void Initialize(GameConfig config);

    /// <summary>Resets the simulation to a clean state.</summary>
    void Reset();

    /// <summary>Releases any resources held by the simulation.</summary>
    void Dispose();

    /// <summary>Advances the simulation by one frame.</summary>
    FrameData Step();

    /// <summary>Returns the current simulation state.</summary>
    FrameData GetCurrentState();

    /// <summary>Loads a previous simulation state.</summary>
    void LoadState(FrameData state);

    /// <summary>Current frame index.</summary>
    int CurrentFrame { get; }

    /// <summary>True when the simulation has completed.</summary>
    bool IsComplete { get; }

    /// <summary>Lifecycle status of the simulation.</summary>
    SimulationStatus Status { get; }
}
