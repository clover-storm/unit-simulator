using System.Numerics;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace UnitSimulator.Core.Tests.Combat;

public class CombatSystemTests
{
    private readonly CombatSystem _combat = new();

    [Fact]
    public void CollectAttackEvents_ShouldGenerateDamageEvents()
    {
        // Arrange
        var events = new FrameEvents();
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
            damage: 10
        );

        var target = new Unit(
            position: new Vector2(10, 0),
            radius: 10f,
            speed: 0f,
            turnSpeed: 0f,
            role: UnitRole.Melee,
            hp: 50,
            id: 2,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.GroundAndAir,
            damage: 0
        );

        var enemies = new List<Unit> { target };

        // Act
        _combat.CollectAttackEvents(attacker, target, enemies, events);

        // Assert: 이벤트가 수집되어야 함, HP는 아직 변경되지 않아야 함
        events.DamageCount.Should().Be(1);
        events.Damages[0].Source.Should().Be(attacker);
        events.Damages[0].Target.Should().Be(target);
        events.Damages[0].Amount.Should().Be(10);
        events.Damages[0].Type.Should().Be(DamageType.Normal);
        target.HP.Should().Be(50); // HP 변경 없음
    }

    [Fact]
    public void CollectAttackEvents_WithSplash_ShouldGenerateMultipleDamageEvents()
    {
        // Arrange
        var events = new FrameEvents();
        var splashAbility = new SplashDamageData { Radius = 40f };

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
            canTarget: TargetType.Ground,
            damage: 10,
            abilities: new List<AbilityData> { splashAbility }
        );

        var primary = new Unit(
            position: new Vector2(10, 0),
            radius: 10f,
            speed: 0f,
            turnSpeed: 0f,
            role: UnitRole.Melee,
            hp: 50,
            id: 2,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 0
        );

        var secondary = new Unit(
            position: new Vector2(20, 0),
            radius: 10f,
            speed: 0f,
            turnSpeed: 0f,
            role: UnitRole.Melee,
            hp: 30,
            id: 3,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 0
        );

        var enemies = new List<Unit> { primary, secondary };

        // Act
        _combat.CollectAttackEvents(attacker, primary, enemies, events);

        // Assert: 주 타겟 + 스플래시 대상 이벤트
        events.DamageCount.Should().Be(2);
        events.Damages.Should().Contain(d => d.Target == primary && d.Type == DamageType.Normal);
        events.Damages.Should().Contain(d => d.Target == secondary && d.Type == DamageType.Splash);
    }

    [Fact]
    public void CreateDeathSpawnRequests_ShouldGenerateSpawnRequests()
    {
        // Arrange
        var deadUnit = new Unit(
            position: new Vector2(100, 100),
            radius: 10f,
            speed: 0f,
            turnSpeed: 0f,
            role: UnitRole.Melee,
            hp: 0,
            id: 1,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 0,
            abilities: new List<AbilityData>
            {
                new DeathSpawnData { SpawnUnitId = "minion", SpawnCount = 2, SpawnRadius = 5f }
            }
        );

        // Act
        var spawns = _combat.CreateDeathSpawnRequests(deadUnit);

        // Assert
        spawns.Should().HaveCount(2);
        spawns.Should().OnlyContain(r => r.UnitId == "minion" && r.Faction == UnitFaction.Enemy);
    }

    [Fact]
    public void ApplyDeathDamage_ShouldDamageNearbyEnemies()
    {
        // Arrange
        var deadUnit = new Unit(
            position: new Vector2(10, 0),
            radius: 10f,
            speed: 0f,
            turnSpeed: 0f,
            role: UnitRole.Melee,
            hp: 0,
            id: 1,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 0,
            abilities: new List<AbilityData>
            {
                new DeathDamageData { Damage = 10, Radius = 30f }
            }
        );
        deadUnit.IsDead = true;

        var nearbyEnemy = new Unit(
            position: new Vector2(20, 0),
            radius: 10f,
            speed: 0f,
            turnSpeed: 0f,
            role: UnitRole.Melee,
            hp: 5,
            id: 2,
            faction: UnitFaction.Friendly,
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 0
        );

        var enemies = new List<Unit> { nearbyEnemy };

        // Act
        var newlyDead = _combat.ApplyDeathDamage(deadUnit, enemies);

        // Assert
        nearbyEnemy.HP.Should().BeLessOrEqualTo(0);
        newlyDead.Should().Contain(nearbyEnemy);
    }

    [Fact]
    public void ChargeAttack_WithSplash_ShouldApplyDoubledDamage()
    {
        // Arrange
        var events = new FrameEvents();
        var chargeAbility = new ChargeAttackData { DamageMultiplier = 2.0f };
        var splashAbility = new SplashDamageData { Radius = 40f };

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
            canTarget: TargetType.Ground,
            damage: 10,
            abilities: new List<AbilityData> { chargeAbility, splashAbility }
        );
        // 강제로 차지 완료 상태 설정
        attacker.EnsureChargeState();
        attacker.ChargeState!.IsCharged = true;

        var primary = new Unit(
            position: new Vector2(10, 0),
            radius: 10f,
            speed: 0f,
            turnSpeed: 0f,
            role: UnitRole.Melee,
            hp: 100,
            id: 2,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 0
        );

        var enemies = new List<Unit> { primary };

        // Act
        _combat.CollectAttackEvents(attacker, primary, enemies, events);

        // Assert: 차지로 인해 2배 데미지 (20)
        events.DamageCount.Should().Be(1);
        events.Damages[0].Amount.Should().Be(20);
    }
}
