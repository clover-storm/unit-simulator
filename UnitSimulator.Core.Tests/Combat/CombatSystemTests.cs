using System.Numerics;
using FluentAssertions;
using Xunit;

namespace UnitSimulator.Core.Tests.Combat;

public class CombatSystemTests
{
    private readonly CombatSystem _combat = new();

    [Fact]
    public void PerformAttack_ShouldTriggerDeathSpawnAndDeathDamage()
    {
        // Arrange: attacker with splash and charge (damage 10 base)
        var attacker = new Unit(
            position: Vector2.Zero,
            radius: 10f,
            speed: 5f,
            turnSpeed: 0.1f,
            role: UnitRole.Melee,
            hp: 100,
            id: 1,
            faction: UnitFaction.Friendly,
            layer: MovementLayer.Ground,
            canTarget: TargetType.GroundAndAir,
            damage: 10,
            abilities: new List<AbilityData> { new SplashDamageData { Radius = 50f }, new ChargeAttackData(), new ShieldData() }
        );

        // Target with death effects
        var target = new Unit(
            position: new Vector2(10, 0),
            radius: 10f,
            speed: 0f,
            turnSpeed: 0f,
            role: UnitRole.Melee,
            hp: 5,
            id: 2,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.GroundAndAir,
            damage: 0,
            abilities: new List<AbilityData>
            {
                new DeathSpawnData { SpawnUnitId = "minion", SpawnCount = 2, SpawnRadius = 5f },
                new DeathDamageData { Damage = 3, Radius = 30f }
            }
        );

        // Another enemy within death explosion radius
        var nearbyEnemy = new Unit(
            position: new Vector2(20, 0),
            radius: 10f,
            speed: 0f,
            turnSpeed: 0f,
            role: UnitRole.Melee,
            hp: 3,
            id: 3,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.GroundAndAir,
            damage: 0
        );

        var enemies = new List<Unit> { target, nearbyEnemy };

        // Act
        var result = _combat.PerformAttack(attacker, target, enemies);

        // Assert: main target dead, death explosion killed the nearby enemy, spawn requests issued
        result.KilledUnits.Should().Contain(target);
        result.KilledUnits.Should().Contain(nearbyEnemy);
        result.SpawnRequests.Should().HaveCount(2);
        result.SpawnRequests.Should().OnlyContain(r => r.UnitId == "minion" && r.Faction == UnitFaction.Enemy);
    }
}
