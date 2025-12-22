using System;
using System.Linq;
using UnitSimulator.Core.Contracts;

namespace UnitSimulator.Core;

/// <summary>
/// Bridges simulator callbacks to a higher-level simulation observer.
/// </summary>
public sealed class SimulationObserverCallbacks : ISimulatorCallbacks
{
    private readonly SimulatorCore _simulator;
    private readonly ISimulationObserver _observer;

    public SimulationObserverCallbacks(SimulatorCore simulator, ISimulationObserver observer)
    {
        _simulator = simulator ?? throw new ArgumentNullException(nameof(simulator));
        _observer = observer ?? throw new ArgumentNullException(nameof(observer));
    }

    public void OnFrameGenerated(FrameData frameData)
    {
        _observer.OnFrameAdvanced(frameData);
    }

    public void OnSimulationComplete(int finalFrameNumber, string reason)
    {
        _observer.OnSimulationComplete(reason);
    }

    public void OnStateChanged(string changeDescription)
    {
        // No direct observer mapping for state change yet.
    }

    public void OnUnitEvent(UnitEventData eventData)
    {
        var unit = FindUnit(eventData.UnitId, eventData.Faction);
        if (unit == null)
        {
            return;
        }

        Unit? relatedUnit = null;
        if (eventData.TargetUnitId.HasValue)
        {
            relatedUnit = FindUnit(eventData.TargetUnitId.Value, eventData.Faction == UnitFaction.Friendly
                ? UnitFaction.Enemy
                : UnitFaction.Friendly);
        }

        switch (eventData.EventType)
        {
            case UnitEventType.Spawned:
                _observer.OnUnitSpawned(unit);
                break;
            case UnitEventType.Died:
                _observer.OnUnitDied(unit, relatedUnit);
                break;
            case UnitEventType.Damaged:
                if (eventData.Value.HasValue)
                {
                    _observer.OnUnitDamaged(unit, eventData.Value.Value, relatedUnit);
                }
                break;
        }
    }

    private Unit? FindUnit(int unitId, UnitFaction faction)
    {
        var squad = faction == UnitFaction.Friendly ? _simulator.FriendlyUnits : _simulator.EnemyUnits;
        return squad.FirstOrDefault(u => u.Id == unitId);
    }
}
