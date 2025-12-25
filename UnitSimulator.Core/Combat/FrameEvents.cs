using System.Numerics;

namespace UnitSimulator;

/// <summary>
/// 피해 유형을 정의합니다.
/// </summary>
public enum DamageType
{
    Normal,       // 일반 공격
    Splash,       // 스플래시 피해
    DeathDamage,  // 사망 시 폭발 피해
    Spell         // 스펠 피해
}

/// <summary>
/// 프레임 내 발생하는 피해 이벤트
/// </summary>
public class DamageEvent
{
    /// <summary>
    /// 피해 원인 유닛 (null일 수 있음 - 스펠 등)
    /// </summary>
    public Unit? Source { get; init; }

    /// <summary>
    /// 피해 대상 유닛
    /// </summary>
    public required Unit Target { get; init; }

    /// <summary>
    /// 피해량
    /// </summary>
    public int Amount { get; init; }

    /// <summary>
    /// 피해 유형
    /// </summary>
    public DamageType Type { get; init; } = DamageType.Normal;
}

/// <summary>
/// 프레임 내 발생하는 모든 이벤트를 수집하는 컨테이너.
/// Phase 1(Collect)에서 이벤트를 수집하고, Phase 2(Apply)에서 일괄 적용합니다.
/// </summary>
public class FrameEvents
{
    /// <summary>
    /// 수집된 피해 이벤트 목록
    /// </summary>
    public List<DamageEvent> Damages { get; } = new();

    /// <summary>
    /// 수집된 스폰 요청 목록
    /// </summary>
    public List<UnitSpawnRequest> Spawns { get; } = new();

    /// <summary>
    /// 피해 이벤트를 추가합니다.
    /// </summary>
    public void AddDamage(Unit source, Unit target, int amount, DamageType type = DamageType.Normal)
    {
        Damages.Add(new DamageEvent
        {
            Source = source,
            Target = target,
            Amount = amount,
            Type = type
        });
    }

    /// <summary>
    /// 스폰 요청을 추가합니다.
    /// </summary>
    public void AddSpawn(UnitSpawnRequest spawn)
    {
        Spawns.Add(spawn);
    }

    /// <summary>
    /// 여러 스폰 요청을 추가합니다.
    /// </summary>
    public void AddSpawns(IEnumerable<UnitSpawnRequest> spawns)
    {
        Spawns.AddRange(spawns);
    }

    /// <summary>
    /// 모든 이벤트를 초기화합니다.
    /// </summary>
    public void Clear()
    {
        Damages.Clear();
        Spawns.Clear();
    }

    /// <summary>
    /// 수집된 피해 이벤트 수
    /// </summary>
    public int DamageCount => Damages.Count;

    /// <summary>
    /// 수집된 스폰 요청 수
    /// </summary>
    public int SpawnCount => Spawns.Count;
}
