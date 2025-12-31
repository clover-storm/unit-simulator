namespace UnitSimulator;

/// <summary>
/// Phase 2: 유닛이 보유할 수 있는 특수 능력 유형
/// </summary>
public enum AbilityType
{
    // === 공격 관련 ===
    /// <summary>돌진 공격 - 일정 거리 이동 후 배율 데미지</summary>
    ChargeAttack,
    /// <summary>범위 공격 - 주변 적에게 피해</summary>
    SplashDamage,
    /// <summary>연쇄 공격 - 근처 적에게 순차 피해</summary>
    ChainDamage,
    /// <summary>관통 공격 - 여러 적 동시 피해</summary>
    PiercingAttack,

    // === 방어 관련 ===
    /// <summary>분리된 쉴드 HP 보유</summary>
    Shield,

    // === 죽음 효과 ===
    /// <summary>죽을 때 유닛 생성</summary>
    DeathSpawn,
    /// <summary>죽을 때 폭발 피해</summary>
    DeathDamage,

    // === 상태 효과 ===
    /// <summary>상태 효과 부여 (스턴, 슬로우, 프리즈 등)</summary>
    StatusEffect
}

/// <summary>
/// 능력의 기본 데이터를 담는 추상 클래스
/// </summary>
public abstract class AbilityData
{
    public AbilityType Type { get; init; }
}

/// <summary>
/// ChargeAttack 능력 데이터
/// 일정 거리 이상 이동 후 공격 시 데미지 배율 적용
/// </summary>
public class ChargeAttackData : AbilityData
{
    public ChargeAttackData()
    {
        Type = AbilityType.ChargeAttack;
    }

    /// <summary>돌진 시작을 위한 최소 거리</summary>
    public float TriggerDistance { get; init; } = 150f;

    /// <summary>돌진 완료를 위한 필요 이동 거리</summary>
    public float RequiredChargeDistance { get; init; } = 100f;

    /// <summary>돌진 완료 시 데미지 배율</summary>
    public float DamageMultiplier { get; init; } = 2.0f;

    /// <summary>돌진 중 이동 속도 배율</summary>
    public float SpeedMultiplier { get; init; } = 2.0f;
}

/// <summary>
/// SplashDamage 능력 데이터
/// 공격 시 주변 적에게도 피해
/// </summary>
public class SplashDamageData : AbilityData
{
    public SplashDamageData()
    {
        Type = AbilityType.SplashDamage;
    }

    /// <summary>스플래시 반경</summary>
    public float Radius { get; init; } = 60f;

    /// <summary>거리별 피해 감소율 (0 = 감소 없음, 1 = 거리 비례 100% 감소)</summary>
    public float DamageFalloff { get; init; } = 0f;
}

/// <summary>
/// Shield 능력 데이터
/// 메인 HP 이전에 소모되는 별도 HP
/// </summary>
public class ShieldData : AbilityData
{
    public ShieldData()
    {
        Type = AbilityType.Shield;
    }

    /// <summary>최대 쉴드 HP</summary>
    public int MaxShieldHP { get; init; } = 200;

    /// <summary>쉴드가 있을 때 스턴 방어 여부</summary>
    public bool BlocksStun { get; init; } = false;

    /// <summary>쉴드가 있을 때 넉백 방어 여부</summary>
    public bool BlocksKnockback { get; init; } = false;
}

/// <summary>
/// DeathSpawn 능력 데이터
/// 유닛 사망 시 다른 유닛 생성
/// </summary>
public class DeathSpawnData : AbilityData
{
    public DeathSpawnData()
    {
        Type = AbilityType.DeathSpawn;
    }

    /// <summary>생성할 유닛의 정의 ID</summary>
    public string SpawnUnitId { get; init; } = "";

    /// <summary>생성할 유닛 수</summary>
    public int SpawnCount { get; init; } = 2;

    /// <summary>생성 범위 반경</summary>
    public float SpawnRadius { get; init; } = 30f;

    /// <summary>생성될 유닛의 HP (0이면 기본값 사용)</summary>
    public int SpawnUnitHP { get; init; } = 0;
}

/// <summary>
/// DeathDamage 능력 데이터
/// 유닛 사망 시 폭발 피해
/// </summary>
public class DeathDamageData : AbilityData
{
    public DeathDamageData()
    {
        Type = AbilityType.DeathDamage;
    }

    /// <summary>폭발 피해량</summary>
    public int Damage { get; init; } = 100;

    /// <summary>폭발 반경</summary>
    public float Radius { get; init; } = 60f;

    /// <summary>넉백 거리 (0이면 넉백 없음)</summary>
    public float KnockbackDistance { get; init; } = 0f;
}

/// <summary>
/// StatusEffect 능력 데이터
/// 대상에게 상태 효과를 부여
/// </summary>
public class StatusEffectAbilityData : AbilityData
{
    public StatusEffectAbilityData()
    {
        Type = AbilityType.StatusEffect;
    }

    /// <summary>부여할 상태 효과 타입 (Flags enum - 여러 효과 동시 적용 가능)</summary>
    public StatusEffectType AppliedEffect { get; init; } = StatusEffectType.None;

    /// <summary>상태 효과 지속 시간 (초)</summary>
    public float EffectDuration { get; init; } = 0f;

    /// <summary>효과 강도/배율 (슬로우 비율, 데미지 배율 등)</summary>
    public float EffectMagnitude { get; init; } = 1.0f;

    /// <summary>효과 적용 범위 (0이면 타겟만)</summary>
    public float EffectRange { get; init; } = 0f;

    /// <summary>영향받는 타겟 타입</summary>
    public TargetType AffectedTargets { get; init; } = TargetType.Ground;
}
