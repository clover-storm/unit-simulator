namespace UnitSimulator.Core.Contracts;

/// <summary>
/// High-level simulation lifecycle status.
/// </summary>
public enum SimulationStatus
{
    /// <summary>Simulation has not been initialized.</summary>
    Uninitialized,
    /// <summary>Simulation is initialized but not running.</summary>
    Initialized,
    /// <summary>Simulation is actively running.</summary>
    Running,
    /// <summary>Simulation completed (all waves cleared or max frames reached).</summary>
    Completed
}
