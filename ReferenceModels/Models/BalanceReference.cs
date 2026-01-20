using System.Text.Json.Serialization;

namespace ReferenceModels.Models;

/// <summary>
/// 게임 밸런스 레퍼런스 데이터.
/// balance.json의 전체 구조를 나타냅니다.
/// </summary>
public class BalanceReference
{
    /// <summary>스키마 버전</summary>
    [JsonPropertyName("version")]
    public int Version { get; set; } = 1;

    /// <summary>시뮬레이션 설정</summary>
    [JsonPropertyName("simulation")]
    public SimulationBalance? Simulation { get; set; }

    /// <summary>유닛 기본 설정</summary>
    [JsonPropertyName("unit")]
    public UnitBalance? Unit { get; set; }

    /// <summary>전투 설정</summary>
    [JsonPropertyName("combat")]
    public CombatBalance? Combat { get; set; }

    /// <summary>분대 행동 설정</summary>
    [JsonPropertyName("squad")]
    public SquadBalance? Squad { get; set; }

    /// <summary>웨이브 설정</summary>
    [JsonPropertyName("wave")]
    public WaveBalance? Wave { get; set; }

    /// <summary>타겟팅 설정</summary>
    [JsonPropertyName("targeting")]
    public TargetingBalance? Targeting { get; set; }

    /// <summary>회피 설정</summary>
    [JsonPropertyName("avoidance")]
    public AvoidanceBalance? Avoidance { get; set; }

    /// <summary>충돌 해소 설정</summary>
    [JsonPropertyName("collision")]
    public CollisionBalance? Collision { get; set; }
}

/// <summary>시뮬레이션 공간 설정</summary>
public class SimulationBalance
{
    [JsonPropertyName("width")]
    public int Width { get; set; } = 3200;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 5100;

    [JsonPropertyName("maxFrames")]
    public int MaxFrames { get; set; } = 3000;

    [JsonPropertyName("frameTimeSeconds")]
    public float FrameTimeSeconds { get; set; } = 1f / 30f;
}

/// <summary>유닛 기본 설정</summary>
public class UnitBalance
{
    [JsonPropertyName("defaultRadius")]
    public float DefaultRadius { get; set; } = 20f;

    [JsonPropertyName("collisionRadiusScale")]
    public float CollisionRadiusScale { get; set; } = 2f / 3f;

    [JsonPropertyName("numAttackSlots")]
    public int NumAttackSlots { get; set; } = 8;

    [JsonPropertyName("slotReevaluateDistance")]
    public float SlotReevaluateDistance { get; set; } = 40f;

    [JsonPropertyName("slotReevaluateIntervalFrames")]
    public int SlotReevaluateIntervalFrames { get; set; } = 60;
}

/// <summary>전투 설정</summary>
public class CombatBalance
{
    [JsonPropertyName("attackCooldown")]
    public float AttackCooldown { get; set; } = 30f;

    [JsonPropertyName("meleeRangeMultiplier")]
    public int MeleeRangeMultiplier { get; set; } = 3;

    [JsonPropertyName("rangedRangeMultiplier")]
    public int RangedRangeMultiplier { get; set; } = 6;

    [JsonPropertyName("engagementTriggerDistanceMultiplier")]
    public float EngagementTriggerDistanceMultiplier { get; set; } = 1.5f;
}

/// <summary>분대 행동 설정</summary>
public class SquadBalance
{
    [JsonPropertyName("rallyDistance")]
    public float RallyDistance { get; set; } = 300f;

    [JsonPropertyName("formationThreshold")]
    public float FormationThreshold { get; set; } = 20f;

    [JsonPropertyName("separationRadius")]
    public float SeparationRadius { get; set; } = 120f;

    [JsonPropertyName("friendlySeparationRadius")]
    public float FriendlySeparationRadius { get; set; } = 80f;

    [JsonPropertyName("destinationThreshold")]
    public float DestinationThreshold { get; set; } = 10f;
}

/// <summary>웨이브 설정</summary>
public class WaveBalance
{
    [JsonPropertyName("maxWaves")]
    public int MaxWaves { get; set; } = 3;
}

/// <summary>타겟팅 설정</summary>
public class TargetingBalance
{
    [JsonPropertyName("reevaluateIntervalFrames")]
    public int ReevaluateIntervalFrames { get; set; } = 45;

    [JsonPropertyName("switchMargin")]
    public float SwitchMargin { get; set; } = 15f;

    [JsonPropertyName("crowdPenaltyPerAttacker")]
    public float CrowdPenaltyPerAttacker { get; set; } = 25f;
}

/// <summary>회피 설정</summary>
public class AvoidanceBalance
{
    [JsonPropertyName("angleStep")]
    public float AngleStep { get; set; } = MathF.PI / 8f;

    [JsonPropertyName("maxIterations")]
    public int MaxIterations { get; set; } = 8;

    [JsonPropertyName("maxLookahead")]
    public float MaxLookahead { get; set; } = 3.5f;
}

/// <summary>충돌 해소 설정</summary>
public class CollisionBalance
{
    [JsonPropertyName("resolutionIterations")]
    public int ResolutionIterations { get; set; } = 3;

    [JsonPropertyName("pushStrength")]
    public float PushStrength { get; set; } = 0.8f;
}
