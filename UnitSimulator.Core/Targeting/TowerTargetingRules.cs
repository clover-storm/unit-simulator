using System.Numerics;

namespace UnitSimulator;

public static class TowerTargetingRules
{
    public static Tower? SelectTowerTarget(Unit unit, IEnumerable<Tower> towers)
    {
        return towers
            .Where(t => !t.IsDestroyed && unit.CanAttackTower(t))
            .OrderBy(t => Vector2.Distance(unit.Position, t.Position))
            .FirstOrDefault();
    }

    public static Unit? SelectUnitTarget(Unit unit, IEnumerable<Unit> enemies)
    {
        return enemies
            .Where(e => !e.IsDead && unit.CanAttack(e))
            .OrderBy(e => Vector2.Distance(unit.Position, e.Position))
            .FirstOrDefault();
    }

    public static (Unit? unitTarget, Tower? towerTarget) SelectTarget(
        Unit unit,
        IEnumerable<Unit> enemies,
        IEnumerable<Tower> towers)
    {
        var livingEnemies = enemies.Where(e => !e.IsDead).ToList();
        var livingTowers = towers.Where(t => !t.IsDestroyed).ToList();

        if (unit.TargetPriority == TargetPriority.Buildings)
        {
            var towerTarget = SelectTowerTarget(unit, livingTowers);
            if (towerTarget != null)
            {
                return (null, towerTarget);
            }

            return (SelectUnitTarget(unit, livingEnemies), null);
        }

        if (livingEnemies.Count > 0)
        {
            return (SelectUnitTarget(unit, livingEnemies), null);
        }

        return (null, SelectTowerTarget(unit, livingTowers));
    }
}
