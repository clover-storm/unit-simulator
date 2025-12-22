using System.Numerics;

namespace UnitSimulator;

/// <summary>
/// Manages wave spawning logic and generates spawn commands.
/// This class no longer directly manipulates the simulation state.
/// Instead, it generates commands that are processed by SimulatorCore.
/// </summary>
public class WaveManager
{
    private int _currentWave = 0;
    private readonly Dictionary<int, List<Vector2>> _waveSpawns = new();

    public int CurrentWave => _currentWave;
    public bool HasMoreWaves => _currentWave < GameConstants.MAX_WAVES;

    public WaveManager()
    {
        InitializeWaveSpawns();
    }

    /// <summary>
    /// Gets spawn commands for the specified wave.
    /// </summary>
    /// <param name="waveNumber">The wave number (1-based).</param>
    /// <param name="frameNumber">The frame number when commands should execute.</param>
    /// <returns>Enumerable of spawn commands for the wave.</returns>
    public IEnumerable<SpawnUnitCommand> GetWaveCommands(int waveNumber, int frameNumber)
    {
        if (!_waveSpawns.TryGetValue(waveNumber, out var spawns))
        {
            yield break;
        }

        foreach (var pos in spawns)
        {
            yield return new SpawnUnitCommand(
                FrameNumber: frameNumber,
                Position: pos,
                Role: UnitRole.Melee,
                Faction: UnitFaction.Enemy,
                HP: GameConstants.ENEMY_HP,
                Speed: 4.0f,
                TurnSpeed: 0.1f
            );
        }
    }

    /// <summary>
    /// Advances to the next wave if possible.
    /// </summary>
    /// <returns>True if advanced, false if no more waves.</returns>
    public bool TryAdvanceWave()
    {
        if (_currentWave < GameConstants.MAX_WAVES)
        {
            _currentWave++;
            Console.WriteLine($"Wave {_currentWave - 1} cleared! Spawning wave {_currentWave}...");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Sets the current wave number directly.
    /// </summary>
    public void SetWave(int wave)
    {
        _currentWave = wave;
    }

    /// <summary>
    /// Spawns the first wave and returns commands.
    /// </summary>
    public IEnumerable<SpawnUnitCommand> SpawnFirstWave(int frameNumber)
    {
        _currentWave = 1;
        return GetWaveCommands(1, frameNumber);
    }

    private void InitializeWaveSpawns()
    {
        int w = GameConstants.SIMULATION_WIDTH;
        int h = GameConstants.SIMULATION_HEIGHT;

        _waveSpawns[1] = new List<Vector2>
        {
            new(w * 0.6f, h * 0.5f - 60),
            new(w * 0.6f, h * 0.5f + 60),
            new(w * 0.65f, h * 0.5f - 120),
            new(w * 0.65f, h * 0.5f + 120),
            new(w * 0.55f, h * 0.5f - 180),
            new(w * 0.55f, h * 0.5f + 180),
        };

        _waveSpawns[2] = new List<Vector2>
        {
            new(w * 0.7f, 150),
            new(w * 0.7f, h - 150),
            new(w * 0.75f, 250),
            new(w * 0.75f, h - 250),
            new(w * 0.8f, h / 2 - 100),
            new(w * 0.8f, h / 2 + 100),
            new(w * 0.85f, h / 2 - 220),
            new(w * 0.85f, h / 2 + 220),
        };

        _waveSpawns[3] = new List<Vector2>
        {
            new(w - 250, h / 2 - 180),
            new(w - 250, h / 2 + 180),
            new(w - 350, h / 2 - 90),
            new(w - 350, h / 2 + 90),
            new(w - 450, h / 2 - 180),
            new(w - 450, h / 2 + 180),
            new(w - 550, h / 2 - 260),
            new(w - 550, h / 2 + 260),
        };
    }
}
