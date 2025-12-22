using UnitSimulator;

namespace UnitSimulator.Core.Contracts;

/// <summary>
/// Receives simulation lifecycle and unit events.
/// </summary>
public interface ISimulationObserver
{
    /// <summary>Called after each simulation frame is advanced.</summary>
    void OnFrameAdvanced(FrameData frameData);

    /// <summary>Called when a unit is spawned.</summary>
    void OnUnitSpawned(Unit unit);

    /// <summary>Called when a unit dies.</summary>
    void OnUnitDied(Unit unit, Unit? killer);

    /// <summary>Called when a unit takes damage.</summary>
    void OnUnitDamaged(Unit unit, int damage, Unit? attacker);

    /// <summary>Called when a wave begins.</summary>
    void OnWaveStarted(int waveNumber);

    /// <summary>Called when the simulation completes.</summary>
    void OnSimulationComplete(string reason);
}
