using System;
using System.Numerics;
using UnitSimulator;
using UnitSimulator.Core.Contracts;

namespace UnitSimulator.Core;

/// <summary>
/// Facade that exposes core simulation through stable contracts.
/// </summary>
public sealed class SimulationFacade : ISimulation, IUnitController
{
    private readonly SimulatorCore _simulator;
    private ISimulatorCallbacks _callbacks;
    private GameConfig? _config;
    private FrameData? _lastFrame;
    private bool _disposed;

    public SimulationFacade(ISimulationObserver? observer = null)
    {
        _simulator = new SimulatorCore();
        _callbacks = observer == null
            ? new DefaultSimulatorCallbacks()
            : new SimulationObserverCallbacks(_simulator, observer);
    }

    public int CurrentFrame => _simulator.CurrentFrame;

    public bool IsComplete
    {
        get
        {
            if (!_simulator.IsInitialized)
            {
                return false;
            }

            var state = GetCurrentState();
            return state.AllWavesCleared || state.MaxFramesReached;
        }
    }

    public SimulationStatus Status
    {
        get
        {
            if (!_simulator.IsInitialized)
            {
                return SimulationStatus.Uninitialized;
            }

            if (IsComplete)
            {
                return SimulationStatus.Completed;
            }

            if (_simulator.IsRunning)
            {
                return SimulationStatus.Running;
            }

            return SimulationStatus.Initialized;
        }
    }

    public void Initialize(GameConfig config)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SimulationFacade));
        }

        _config = config ?? throw new ArgumentNullException(nameof(config));

        _simulator.Initialize();
        _simulator.CurrentWave = config.InitialWave;
        _simulator.HasMoreWaves = config.HasMoreWaves;
    }

    public void Reset()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SimulationFacade));
        }

        _simulator.Reset();

        if (_config != null)
        {
            _simulator.CurrentWave = _config.InitialWave;
            _simulator.HasMoreWaves = _config.HasMoreWaves;
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _simulator.Stop();
        _disposed = true;
    }

    public FrameData Step()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SimulationFacade));
        }

        _lastFrame = _simulator.Step(_callbacks);
        return _lastFrame;
    }

    public FrameData GetCurrentState()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SimulationFacade));
        }

        _lastFrame = _simulator.GetCurrentFrameData();
        return _lastFrame;
    }

    public void LoadState(FrameData state)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SimulationFacade));
        }

        _simulator.LoadState(state, _callbacks);
        _lastFrame = state;
    }

    public void SpawnUnit(Vector2 position, UnitRole role, UnitFaction faction, int? hp = null, float? speed = null, float? turnSpeed = null)
    {
        Enqueue(new SpawnUnitCommand(CurrentFrame, position, role, faction, hp, speed, turnSpeed));
    }

    public void MoveUnit(int unitId, UnitFaction faction, Vector2 destination)
    {
        Enqueue(new MoveUnitCommand(CurrentFrame, unitId, faction, destination));
    }

    public void DamageUnit(int unitId, UnitFaction faction, int damage)
    {
        Enqueue(new DamageUnitCommand(CurrentFrame, unitId, faction, damage));
    }

    public void KillUnit(int unitId, UnitFaction faction)
    {
        Enqueue(new KillUnitCommand(CurrentFrame, unitId, faction));
    }

    public void ReviveUnit(int unitId, UnitFaction faction, int hp)
    {
        Enqueue(new ReviveUnitCommand(CurrentFrame, unitId, faction, hp));
    }

    public void SetUnitHealth(int unitId, UnitFaction faction, int hp)
    {
        Enqueue(new SetUnitHealthCommand(CurrentFrame, unitId, faction, hp));
    }

    public void RemoveUnit(int unitId, UnitFaction faction)
    {
        Enqueue(new RemoveUnitCommand(CurrentFrame, unitId, faction));
    }

    private void Enqueue(ISimulationCommand command)
    {
        _simulator.EnqueueCommand(command);
    }
}
