using System.Text.Json.Serialization;
using ReferenceModels.Models.Enums;

namespace ReferenceModels.Models;

/// <summary>
/// JSON에서 로드되는 스킬 레퍼런스 데이터.
/// type 필드로 실제 AbilityData 타입을 결정합니다.
/// </summary>
public class SkillReference
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "";

    // ChargeAttack
    [JsonPropertyName("triggerDistance")]
    public float TriggerDistance { get; init; } = 150f;

    [JsonPropertyName("requiredChargeDistance")]
    public float RequiredChargeDistance { get; init; } = 100f;

    [JsonPropertyName("damageMultiplier")]
    public float DamageMultiplier { get; init; } = 2.0f;

    [JsonPropertyName("speedMultiplier")]
    public float SpeedMultiplier { get; init; } = 2.0f;

    // SplashDamage
    [JsonPropertyName("radius")]
    public float Radius { get; init; } = 60f;

    [JsonPropertyName("damageFalloff")]
    public float DamageFalloff { get; init; } = 0f;

    // Shield
    [JsonPropertyName("maxShieldHP")]
    public int MaxShieldHP { get; init; } = 200;

    [JsonPropertyName("blocksStun")]
    public bool BlocksStun { get; init; } = false;

    [JsonPropertyName("blocksKnockback")]
    public bool BlocksKnockback { get; init; } = false;

    // DeathSpawn
    [JsonPropertyName("spawnUnitId")]
    public string SpawnUnitId { get; init; } = "";

    [JsonPropertyName("spawnCount")]
    public int SpawnCount { get; init; } = 2;

    [JsonPropertyName("spawnRadius")]
    public float SpawnRadius { get; init; } = 30f;

    [JsonPropertyName("spawnUnitHP")]
    public int SpawnUnitHP { get; init; } = 0;

    // DeathDamage
    [JsonPropertyName("damage")]
    public int Damage { get; init; } = 100;

    [JsonPropertyName("knockbackDistance")]
    public float KnockbackDistance { get; init; } = 0f;

    // === 상태 효과 관련 (새로운 필드들) ===

    /// <summary>부여할 상태 효과 (기본값: None)</summary>
    [JsonPropertyName("appliedEffect")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public StatusEffectType AppliedEffect { get; init; } = StatusEffectType.None;

    /// <summary>상태 효과 지속 시간 (초, 기본값: 0)</summary>
    [JsonPropertyName("effectDuration")]
    public float EffectDuration { get; init; } = 0f;

    /// <summary>효과 크기 (슬로우: 속도 감소율, 레이지: 속도 증가율 등)</summary>
    [JsonPropertyName("effectMagnitude")]
    public float EffectMagnitude { get; init; } = 0f;

    /// <summary>효과 범위 (기본값: 0, 범위 효과용)</summary>
    [JsonPropertyName("effectRange")]
    public float EffectRange { get; init; } = 0f;

    /// <summary>영향받는 대상 (기본값: None)</summary>
    [JsonPropertyName("affectedTargets")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public TargetType AffectedTargets { get; init; } = TargetType.None;
}
