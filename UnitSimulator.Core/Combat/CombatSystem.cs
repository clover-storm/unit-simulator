using System.Numerics;
using System.Linq;
using System.Collections.Generic;

namespace UnitSimulator;

/// <summary>
/// Phase 2: 전투 관련 로직을 처리하는 시스템
/// SplashDamage, ChargeAttack, DeathSpawn 등의 능력을 처리
///
/// 2-Phase Update 패턴:
/// - Phase 1 (Collect): CollectAttackEvents()로 DamageEvent 수집
/// - Phase 2 (Apply): SimulatorCore에서 이벤트 일괄 적용 및 사망 처리
/// </summary>
public class CombatSystem
{
    // ════════════════════════════════════════════════════════════════════════
    // Phase 1: Collect - 이벤트 수집 (상태 변경 없음)
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 공격에 대한 피해 이벤트를 수집합니다. (Phase 1)
    /// 실제 HP 변경은 Phase 2에서 일괄 적용됩니다.
    /// </summary>
    /// <param name="attacker">공격자</param>
    /// <param name="target">주 타겟</param>
    /// <param name="allEnemies">스플래시 대상이 될 수 있는 모든 적</param>
    /// <param name="events">이벤트를 수집할 컨테이너</param>
    public void CollectAttackEvents(Unit attacker, Unit target, List<Unit> allEnemies, FrameEvents events)
    {
        if (target == null || target.IsDead) return;

        int damage = attacker.GetEffectiveDamage();

        // 주 타겟에 대한 피해 이벤트 추가
        events.AddDamage(attacker, target, damage, DamageType.Normal);

        // SplashDamage 이벤트 수집
        var splashData = attacker.GetAbility<SplashDamageData>();
        if (splashData != null)
        {
            CollectSplashDamage(attacker, target, damage, splashData, allEnemies, events);
        }

        // 공격 후 처리 (돌진 상태 소비 등) - 이건 Phase 1에서 허용
        attacker.OnAttackPerformed();
    }

    /// <summary>
    /// 스플래시 피해 이벤트를 수집합니다. (Phase 1)
    /// </summary>
    private void CollectSplashDamage(Unit attacker, Unit mainTarget, int baseDamage, SplashDamageData splashData, List<Unit> allEnemies, FrameEvents events)
    {
        foreach (var enemy in allEnemies)
        {
            if (enemy == mainTarget || enemy.IsDead) continue;
            if (!attacker.CanAttack(enemy)) continue;

            float distance = Vector2.Distance(mainTarget.Position, enemy.Position);
            if (distance > splashData.Radius) continue;

            // 거리에 따른 피해 감소 계산
            int splashDamage = baseDamage;
            if (splashData.DamageFalloff > 0)
            {
                float falloffFactor = 1f - (distance / splashData.Radius) * splashData.DamageFalloff;
                splashDamage = (int)(baseDamage * Math.Max(0, falloffFactor));
            }

            if (splashDamage > 0)
            {
                events.AddDamage(attacker, enemy, splashDamage, DamageType.Splash);
            }
        }
    }

    // ════════════════════════════════════════════════════════════════════════
    // Phase 2: Death Processing - SimulatorCore에서 호출
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 사망한 유닛의 DeathSpawn 요청을 생성합니다. (Phase 2)
    /// </summary>
    public List<UnitSpawnRequest> CreateDeathSpawnRequests(Unit deadUnit)
    {
        var spawns = new List<UnitSpawnRequest>();

        var deathSpawn = deadUnit.GetAbility<DeathSpawnData>();
        if (deathSpawn == null || deathSpawn.SpawnCount <= 0) return spawns;

        for (int i = 0; i < deathSpawn.SpawnCount; i++)
        {
            float angle = (2 * MathF.PI / deathSpawn.SpawnCount) * i;
            Vector2 offset = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * deathSpawn.SpawnRadius;
            Vector2 spawnPos = deadUnit.Position + offset;

            spawns.Add(new UnitSpawnRequest
            {
                UnitId = deathSpawn.SpawnUnitId,
                Position = spawnPos,
                Faction = deadUnit.Faction,
                HP = deathSpawn.SpawnUnitHP
            });
        }

        return spawns;
    }

    /// <summary>
    /// 사망한 유닛의 DeathDamage를 주변 적에게 적용합니다. (Phase 2)
    /// </summary>
    /// <returns>DeathDamage로 사망한 유닛 목록</returns>
    public List<Unit> ApplyDeathDamage(Unit deadUnit, List<Unit> enemies)
    {
        var newlyDead = new List<Unit>();

        var deathDamage = deadUnit.GetAbility<DeathDamageData>();
        if (deathDamage == null || deathDamage.Damage <= 0) return newlyDead;

        foreach (var enemy in enemies)
        {
            if (enemy.IsDead) continue;

            float distance = Vector2.Distance(deadUnit.Position, enemy.Position);
            if (distance > deathDamage.Radius) continue;

            bool wasAlive = !enemy.IsDead;
            enemy.TakeDamage(deathDamage.Damage);

            // 넉백 적용
            if (deathDamage.KnockbackDistance > 0 && !enemy.IsDead)
            {
                Vector2 knockbackDir = Vector2.Normalize(enemy.Position - deadUnit.Position);
                enemy.Position += knockbackDir * deathDamage.KnockbackDistance;
            }

            // TakeDamage()가 IsDead를 설정하므로, 이전 상태와 비교
            if (wasAlive && enemy.IsDead)
            {
                newlyDead.Add(enemy);
            }
        }

        return newlyDead;
    }

    // ════════════════════════════════════════════════════════════════════════
    // 기타 유틸리티
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// 유닛의 돌진 상태를 업데이트합니다.
    /// </summary>
    public void UpdateChargeState(Unit unit, Unit? target)
    {
        if (unit.ChargeState == null) return;

        var chargeData = unit.GetAbility<ChargeAttackData>();
        if (chargeData == null) return;

        // 타겟이 없거나 죽었으면 돌진 리셋
        if (target == null || target.IsDead)
        {
            unit.ChargeState.Reset();
            return;
        }

        float distanceToTarget = Vector2.Distance(unit.Position, target.Position);

        // 돌진 시작 조건: 타겟과의 거리가 트리거 거리 이상
        if (!unit.ChargeState.IsCharging && distanceToTarget >= chargeData.TriggerDistance)
        {
            unit.ChargeState.StartCharge(unit.Position, chargeData.RequiredChargeDistance);
        }

        // 돌진 중이면 거리 업데이트
        if (unit.ChargeState.IsCharging)
        {
            unit.ChargeState.UpdateChargeDistance(unit.Position);

            // 공격 범위 내에 들어오면 돌진 완료 상태 유지 (공격 시 소비됨)
            if (distanceToTarget <= unit.AttackRange)
            {
                // 이미 IsCharged가 설정되어 있으면 다음 공격에서 배율 적용
            }
        }
    }

}

/// <summary>
/// 유닛 생성 요청 데이터
/// </summary>
public class UnitSpawnRequest
{
    public string UnitId { get; init; } = "";
    public Vector2 Position { get; init; }
    public UnitFaction Faction { get; init; }
    public int HP { get; init; }
}

/// <summary>
/// 공격 결과 데이터
/// </summary>
public class AttackResult
{
    public List<Unit> KilledUnits { get; } = new();
    public List<UnitSpawnRequest> SpawnRequests { get; } = new();
}
