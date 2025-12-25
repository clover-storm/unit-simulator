using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace UnitSimulator.Core.Tests.Combat;

/// <summary>
/// 2-Phase Update 패턴의 연쇄 사망 처리를 테스트합니다.
/// </summary>
public class ChainDeathTests
{
    [Fact]
    public void ChainDeath_DeathDamageKillsAnotherUnit_ShouldTriggerSecondDeathSpawn()
    {
        // Arrange: B가 죽으면 DeathDamage로 C를 죽이고, C가 DeathSpawn
        var combat = new CombatSystem();
        var events = new FrameEvents();

        // 타겟 B (DeathDamage 보유 - 죽으면 50 범위 내 100 데미지)
        var targetB = new Unit(
            position: new Vector2(100, 100),
            radius: 10f,
            speed: 0f,
            turnSpeed: 0f,
            role: UnitRole.Melee,
            hp: 5,  // 곧 죽음
            id: 100,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 0,
            abilities: new List<AbilityData>
            {
                new DeathDamageData { Damage = 100, Radius = 50f }
            }
        );

        // 타겟 C (DeathSpawn 보유 - 죽으면 minion 2개 스폰)
        var targetC = new Unit(
            position: new Vector2(120, 100),  // B에서 20 거리 (DeathDamage 범위 내)
            radius: 10f,
            speed: 0f,
            turnSpeed: 0f,
            role: UnitRole.Melee,
            hp: 10,  // B의 DeathDamage(100)로 죽음
            id: 101,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 0,
            abilities: new List<AbilityData>
            {
                new DeathSpawnData { SpawnUnitId = "minion", SpawnCount = 2, SpawnRadius = 5f }
            }
        );

        var enemies = new List<Unit> { targetB, targetC };

        // B를 직접 죽임 (외부 공격으로 가정)
        targetB.TakeDamage(100);

        // 큐 기반 연쇄 사망 처리
        var deathQueue = new Queue<Unit>();
        var processed = new HashSet<Unit>();

        deathQueue.Enqueue(targetB);

        while (deathQueue.Count > 0)
        {
            var dead = deathQueue.Dequeue();
            if (processed.Contains(dead)) continue;

            if (!dead.IsDead)
            {
                dead.IsDead = true;
            }
            processed.Add(dead);

            // DeathSpawn
            var spawns = combat.CreateDeathSpawnRequests(dead);
            events.AddSpawns(spawns);

            // DeathDamage
            var newlyDead = combat.ApplyDeathDamage(dead, enemies);
            foreach (var killed in newlyDead)
            {
                if (!processed.Contains(killed))
                {
                    deathQueue.Enqueue(killed);
                }
            }
        }

        // Assert
        targetB.IsDead.Should().BeTrue("B가 죽어야 함");
        targetC.IsDead.Should().BeTrue("C가 B의 DeathDamage로 죽어야 함");
        events.SpawnCount.Should().Be(2, "C의 DeathSpawn으로 2개 유닛이 생성되어야 함");
        events.Spawns.Should().OnlyContain(s => s.UnitId == "minion", "스폰된 유닛은 모두 minion이어야 함");
    }

    [Fact]
    public void ChainDeath_ThreeLevelChain_ShouldProcessAllDeaths()
    {
        // Arrange: A→B(DeathDamage)→C(DeathDamage)→D(DeathSpawn)
        var combat = new CombatSystem();
        var events = new FrameEvents();

        var unitB = new Unit(
            position: new Vector2(100, 100),
            radius: 10f, speed: 0f, turnSpeed: 0f,
            role: UnitRole.Melee, hp: 5, id: 1,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 0,
            abilities: new List<AbilityData>
            {
                new DeathDamageData { Damage = 100, Radius = 50f }
            }
        );

        var unitC = new Unit(
            position: new Vector2(120, 100),  // B에서 20 거리
            radius: 10f, speed: 0f, turnSpeed: 0f,
            role: UnitRole.Melee, hp: 10, id: 2,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 0,
            abilities: new List<AbilityData>
            {
                new DeathDamageData { Damage = 100, Radius = 50f }
            }
        );

        var unitD = new Unit(
            position: new Vector2(140, 100),  // C에서 20 거리
            radius: 10f, speed: 0f, turnSpeed: 0f,
            role: UnitRole.Melee, hp: 10, id: 3,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 0,
            abilities: new List<AbilityData>
            {
                new DeathSpawnData { SpawnUnitId = "skeleton", SpawnCount = 3, SpawnRadius = 5f }
            }
        );

        var enemies = new List<Unit> { unitB, unitC, unitD };

        // B를 직접 죽임
        unitB.TakeDamage(100);

        // 큐 기반 연쇄 사망 처리
        var deathQueue = new Queue<Unit>();
        var processed = new HashSet<Unit>();

        deathQueue.Enqueue(unitB);

        while (deathQueue.Count > 0)
        {
            var dead = deathQueue.Dequeue();
            if (processed.Contains(dead)) continue;

            if (!dead.IsDead)
            {
                dead.IsDead = true;
            }
            processed.Add(dead);

            var spawns = combat.CreateDeathSpawnRequests(dead);
            events.AddSpawns(spawns);

            var newlyDead = combat.ApplyDeathDamage(dead, enemies);
            foreach (var killed in newlyDead)
            {
                if (!processed.Contains(killed))
                {
                    deathQueue.Enqueue(killed);
                }
            }
        }

        // Assert: 3단계 연쇄 사망
        unitB.IsDead.Should().BeTrue("B가 죽어야 함");
        unitC.IsDead.Should().BeTrue("C가 B의 DeathDamage로 죽어야 함");
        unitD.IsDead.Should().BeTrue("D가 C의 DeathDamage로 죽어야 함");
        events.SpawnCount.Should().Be(3, "D의 DeathSpawn으로 3개 skeleton 생성");
        processed.Should().HaveCount(3, "3개 유닛 모두 처리됨");
    }

    [Fact]
    public void ChainDeath_NoInfiniteLoop_WhenDeathDamageTargetsSameFaction()
    {
        // Arrange: DeathDamage가 같은 팩션을 대상으로 하지 않음
        var combat = new CombatSystem();
        var events = new FrameEvents();

        var unitA = new Unit(
            position: new Vector2(100, 100),
            radius: 10f, speed: 0f, turnSpeed: 0f,
            role: UnitRole.Melee, hp: 5, id: 1,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 0,
            abilities: new List<AbilityData>
            {
                new DeathDamageData { Damage = 100, Radius = 50f }
            }
        );

        var unitB = new Unit(
            position: new Vector2(120, 100),
            radius: 10f, speed: 0f, turnSpeed: 0f,
            role: UnitRole.Melee, hp: 5, id: 2,
            faction: UnitFaction.Enemy,  // 같은 팩션
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 0,
            abilities: new List<AbilityData>
            {
                new DeathDamageData { Damage = 100, Radius = 50f }
            }
        );

        // 같은 팩션이므로 DeathDamage 대상에서 제외됨
        var opposingUnits = new List<Unit>();  // 반대편 유닛 없음

        unitA.TakeDamage(100);

        var deathQueue = new Queue<Unit>();
        var processed = new HashSet<Unit>();
        deathQueue.Enqueue(unitA);

        int iterations = 0;
        const int maxIterations = 100;

        while (deathQueue.Count > 0 && iterations < maxIterations)
        {
            iterations++;
            var dead = deathQueue.Dequeue();
            if (processed.Contains(dead)) continue;

            dead.IsDead = true;
            processed.Add(dead);

            // 같은 팩션 유닛은 DeathDamage 대상에서 제외
            var newlyDead = combat.ApplyDeathDamage(dead, opposingUnits);
            foreach (var killed in newlyDead)
            {
                deathQueue.Enqueue(killed);
            }
        }

        // Assert: 무한 루프 없이 완료
        iterations.Should().BeLessThan(maxIterations, "무한 루프가 발생하지 않아야 함");
        processed.Should().HaveCount(1, "A만 처리됨");
        unitB.IsDead.Should().BeFalse("B는 같은 팩션이므로 죽지 않음");
    }
}
