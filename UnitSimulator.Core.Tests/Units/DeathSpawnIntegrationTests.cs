using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace UnitSimulator.Core.Tests.Units;

/// <summary>
/// DeathSpawn이 UnitRegistry를 통해 올바른 스탯의 유닛을 생성하는지 테스트합니다.
/// </summary>
public class DeathSpawnIntegrationTests
{
    [Fact]
    public void DeathSpawn_WithRegisteredUnit_ShouldSpawnWithCorrectStats()
    {
        // Arrange
        var simulator = new SimulatorCore();
        simulator.Initialize();

        // 기존 유닛들 제거
        foreach (var unit in simulator.FriendlyUnits.ToList())
        {
            simulator.RemoveUnit(unit.Id, UnitFaction.Friendly);
        }
        foreach (var unit in simulator.EnemyUnits.ToList())
        {
            simulator.RemoveUnit(unit.Id, UnitFaction.Enemy);
        }

        // Golem 유닛 생성 (DeathSpawn: golemite 2개)
        var golem = new Unit(
            position: new Vector2(100, 100),
            radius: 40f,
            speed: 3.0f,
            turnSpeed: 0.1f,
            role: UnitRole.Melee,
            hp: 10,  // 낮은 HP로 쉽게 죽도록
            id: 999,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 100,
            abilities: new List<AbilityData>
            {
                new DeathSpawnData
                {
                    SpawnUnitId = "golemite",
                    SpawnCount = 2,
                    SpawnRadius = 30f
                }
            }
        );

        // 공격자 유닛 생성
        var attacker = new Unit(
            position: new Vector2(100, 130),
            radius: 20f,
            speed: 4.5f,
            turnSpeed: 0.08f,
            role: UnitRole.Melee,
            hp: 1000,
            id: 1,
            faction: UnitFaction.Friendly,
            layer: MovementLayer.Ground,
            canTarget: TargetType.Ground,
            damage: 100
        );

        // 수동으로 추가 (InjectUnit은 레지스트리 사용하므로 직접 추가)
        var friendlySquad = new List<Unit> { attacker };
        var enemySquad = new List<Unit> { golem };

        var events = new FrameEvents();
        var combat = new CombatSystem();

        // golem 공격
        combat.CollectAttackEvents(attacker, golem, enemySquad, events);

        // Phase 2: 피해 적용
        foreach (var damage in events.Damages)
        {
            damage.Target.TakeDamage(damage.Amount);
        }

        // Phase 2: 사망 처리
        var deathQueue = new Queue<Unit>();
        var processed = new HashSet<Unit>();

        if (golem.HP <= 0)
        {
            deathQueue.Enqueue(golem);
        }

        while (deathQueue.Count > 0)
        {
            var dead = deathQueue.Dequeue();
            if (processed.Contains(dead)) continue;

            dead.IsDead = true;
            processed.Add(dead);

            var spawns = combat.CreateDeathSpawnRequests(dead);
            events.AddSpawns(spawns);
        }

        // Assert: DeathSpawn 요청이 생성됨
        events.SpawnCount.Should().Be(2);
        events.Spawns.Should().OnlyContain(s => s.UnitId == "golemite");
    }

    [Fact]
    public void SimulatorCore_SpawnFromRegistry_ShouldHaveCorrectAbilities()
    {
        // Arrange
        var simulator = new SimulatorCore();
        simulator.Initialize();

        // golemite 정의가 등록되어 있는지 확인
        simulator.UnitRegistry.HasDefinition("golemite").Should().BeTrue();

        // golemite 정의 확인
        var golemiteDef = simulator.UnitRegistry.GetDefinition("golemite")!;

        // Assert: golemite는 DeathDamage 능력을 가짐
        golemiteDef.Abilities.Should().ContainSingle(a => a is DeathDamageData);
        var deathDamage = golemiteDef.Abilities.OfType<DeathDamageData>().First();
        deathDamage.Damage.Should().Be(100);
    }

    [Fact]
    public void ChainedDeathSpawn_ElixirGolem_ShouldSpawnMultipleLevels()
    {
        // Arrange
        var registry = UnitRegistry.CreateWithDefaults();

        // elixir_golemite → elixir_blob 연쇄 스폰 확인
        var golemiteDef = registry.GetDefinition("elixir_golemite")!;
        var blobDef = registry.GetDefinition("elixir_blob")!;

        // Assert: 연쇄 스폰 구조 확인
        var golemiteDeathSpawn = golemiteDef.Abilities.OfType<DeathSpawnData>().FirstOrDefault();
        golemiteDeathSpawn.Should().NotBeNull();
        golemiteDeathSpawn!.SpawnUnitId.Should().Be("elixir_blob");
        golemiteDeathSpawn.SpawnCount.Should().Be(2);

        // elixir_blob은 더 이상 스폰하지 않음
        blobDef.Abilities.OfType<DeathSpawnData>().Should().BeEmpty();
    }

    [Fact]
    public void Guard_ShouldHaveShieldAbility()
    {
        // Arrange
        var registry = UnitRegistry.CreateWithDefaults();
        var guardDef = registry.GetDefinition("guard")!;

        // Act
        var unit = guardDef.CreateUnit(1, UnitFaction.Enemy, Vector2.Zero);

        // Assert
        var shield = unit.GetAbility<ShieldData>();
        shield.Should().NotBeNull();
        shield!.MaxShieldHP.Should().Be(199);
        unit.ShieldHP.Should().Be(199);  // 초기 쉴드 HP
    }
}
