using UnitSimulator;

namespace UnitSimulator.Core.Contracts;

/// <summary>
/// Provides game data to the simulation at runtime.
/// </summary>
public interface IDataProvider
{
    /// <summary>Returns stats for a unit role/faction pair.</summary>
    UnitStats GetUnitStats(UnitRole role, UnitFaction faction);

    /// <summary>Returns a wave definition by wave number.</summary>
    WaveDefinition GetWaveDefinition(int waveNumber);

    /// <summary>Returns balance tuning values.</summary>
    GameBalance GetGameBalance();
}
