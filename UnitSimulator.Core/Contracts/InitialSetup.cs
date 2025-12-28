using System.Collections.Generic;
using System.Numerics;

namespace UnitSimulator.Core.Contracts;

/// <summary>
/// 게임 시작 시 초기 상태를 정의합니다.
/// 타워 배치, 초기 유닛, 게임 설정 등을 포함합니다.
/// </summary>
public sealed class InitialSetup
{
    /// <summary>
    /// 타워 초기 설정 목록 (양 진영 모두 포함)
    /// </summary>
    public List<TowerSetup> Towers { get; init; } = new();

    /// <summary>
    /// 초기 유닛 스폰 요청 목록 (테스트/튜토리얼용)
    /// 클래시 로열 표준에서는 비어있음
    /// </summary>
    public List<UnitSpawnSetup> InitialUnits { get; init; } = new();

    /// <summary>
    /// 게임 시간 설정 (선택적)
    /// </summary>
    public GameTimeSetup? GameTime { get; init; }

    /// <summary>
    /// 클래시 로열 표준 레이아웃 생성
    /// 타워 6개, 초기 유닛 없음
    /// </summary>
    public static InitialSetup CreateClashRoyaleStandard()
    {
        return new InitialSetup
        {
            Towers = TowerSetupDefaults.ClashRoyaleStandard(),
            InitialUnits = new List<UnitSpawnSetup>(),
            GameTime = new GameTimeSetup()
        };
    }
}

/// <summary>
/// 개별 타워 초기 설정
/// </summary>
public sealed class TowerSetup
{
    /// <summary>
    /// 타워 유형 (King / Princess)
    /// </summary>
    public TowerType Type { get; init; }

    /// <summary>
    /// 소속 진영
    /// </summary>
    public UnitFaction Faction { get; init; }

    /// <summary>
    /// 타워 위치 (null이면 기본 위치 사용)
    /// </summary>
    public Vector2? Position { get; init; }

    /// <summary>
    /// 초기 HP (null이면 최대 HP)
    /// </summary>
    public int? InitialHP { get; init; }

    /// <summary>
    /// King 타워 활성화 여부 (null이면 기본값: Princess=true, King=false)
    /// </summary>
    public bool? IsActivated { get; init; }
}

/// <summary>
/// 초기 유닛 스폰 설정 (테스트/튜토리얼용)
/// </summary>
public sealed class UnitSpawnSetup
{
    /// <summary>
    /// 유닛 참조 ID (ReferenceManager에서 조회)
    /// </summary>
    public required string UnitId { get; init; }

    /// <summary>
    /// 소속 진영
    /// </summary>
    public UnitFaction Faction { get; init; }

    /// <summary>
    /// 스폰 위치
    /// </summary>
    public Vector2 Position { get; init; }

    /// <summary>
    /// HP 오버라이드 (null이면 참조 데이터 사용)
    /// </summary>
    public int? HP { get; init; }

    /// <summary>
    /// 스폰 수량 (기본 1)
    /// </summary>
    public int Count { get; init; } = 1;

    /// <summary>
    /// 스폰 반경 (Count > 1일 때 분산 배치)
    /// </summary>
    public float SpawnRadius { get; init; } = 30f;
}

/// <summary>
/// 게임 시간 설정
/// </summary>
public sealed class GameTimeSetup
{
    /// <summary>
    /// 정규 시간 (초), 기본 180초 (3분)
    /// </summary>
    public float RegularTime { get; init; } = 180f;

    /// <summary>
    /// 최대 게임 시간 (초), 기본 300초 (5분)
    /// </summary>
    public float MaxGameTime { get; init; } = 300f;
}

/// <summary>
/// 타워 기본 설정 프리셋
/// </summary>
public static class TowerSetupDefaults
{
    /// <summary>
    /// 클래시 로열 표준 6타워 배치
    /// King Tower 1개 + Princess Tower 2개 × 2진영
    /// </summary>
    public static List<TowerSetup> ClashRoyaleStandard()
    {
        return new List<TowerSetup>
        {
            // Friendly
            new TowerSetup { Type = TowerType.King, Faction = UnitFaction.Friendly },
            new TowerSetup
            {
                Type = TowerType.Princess,
                Faction = UnitFaction.Friendly,
                Position = MapLayout.FriendlyPrincessLeftPosition
            },
            new TowerSetup
            {
                Type = TowerType.Princess,
                Faction = UnitFaction.Friendly,
                Position = MapLayout.FriendlyPrincessRightPosition
            },
            // Enemy
            new TowerSetup { Type = TowerType.King, Faction = UnitFaction.Enemy },
            new TowerSetup
            {
                Type = TowerType.Princess,
                Faction = UnitFaction.Enemy,
                Position = MapLayout.EnemyPrincessLeftPosition
            },
            new TowerSetup
            {
                Type = TowerType.Princess,
                Faction = UnitFaction.Enemy,
                Position = MapLayout.EnemyPrincessRightPosition
            },
        };
    }
}
