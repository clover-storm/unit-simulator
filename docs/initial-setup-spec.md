# Initial Setup Specification

게임 초기 상태(타워, 유닛)를 외부에서 정의하는 시스템 스펙입니다.

## 1. 설계 원칙

| 원칙 | 설명 |
|------|------|
| **코어는 실행만** | SimulatorCore는 생성 규칙을 갖지 않음 |
| **외부 주입** | 초기 타워/유닛은 `InitialSetup`으로 전달 |
| **데이터 드리븐** | `ReferenceManager`를 통한 유닛 정의 사용 |

## 2. 클래시 로열 기준

게임 시작 시:
- **타워**: King 1 + Princess 2 × 2진영 = 6개
- **초기 유닛**: 없음 (카드로 스폰)

## 3. 데이터 구조

### InitialSetup

```csharp
public sealed class InitialSetup
{
    public List<TowerSetup> Towers { get; init; }
    public List<UnitSpawnSetup> InitialUnits { get; init; }
    public GameTimeSetup? GameTime { get; init; }

    public static InitialSetup CreateClashRoyaleStandard();
}
```

### TowerSetup

```csharp
public sealed class TowerSetup
{
    public TowerType Type { get; init; }        // King / Princess
    public UnitFaction Faction { get; init; }   // Friendly / Enemy
    public Vector2? Position { get; init; }     // null이면 기본 위치
    public int? InitialHP { get; init; }        // null이면 최대 HP
    public bool? IsActivated { get; init; }     // King Tower 활성화 여부
}
```

### UnitSpawnSetup

```csharp
public sealed class UnitSpawnSetup
{
    public required string UnitId { get; init; }  // ReferenceManager에서 조회
    public UnitFaction Faction { get; init; }
    public Vector2 Position { get; init; }
    public int? HP { get; init; }                 // HP 오버라이드
    public int Count { get; init; } = 1;          // 스폰 수량
    public float SpawnRadius { get; init; } = 30f; // 분산 배치 반경
}
```

### GameTimeSetup

```csharp
public sealed class GameTimeSetup
{
    public float RegularTime { get; init; } = 180f;   // 정규 시간 (3분)
    public float MaxGameTime { get; init; } = 300f;   // 최대 시간 (5분)
}
```

## 4. 기본 프리셋

### 클래시 로열 표준

```csharp
TowerSetupDefaults.ClashRoyaleStandard()
// Returns:
// - Friendly: King, Princess Left, Princess Right
// - Enemy: King, Princess Left, Princess Right
```

## 5. 사용 예시

### 기본 사용 (클래시 로열 표준)

```csharp
var simulator = new SimulatorCore();
simulator.Initialize();  // 클래시 로열 표준 사용
```

### 커스텀 설정

```csharp
var setup = new InitialSetup
{
    Towers = new List<TowerSetup>
    {
        new() { Type = TowerType.King, Faction = UnitFaction.Friendly },
        new() { Type = TowerType.King, Faction = UnitFaction.Enemy, InitialHP = 1000 }
    },
    InitialUnits = new List<UnitSpawnSetup>
    {
        new() { UnitId = "knight", Faction = UnitFaction.Friendly,
                Position = new Vector2(1600, 1800) },
        new() { UnitId = "golem", Faction = UnitFaction.Enemy,
                Position = new Vector2(1600, 3300) }
    }
};

simulator.Initialize(setup);
```

### 테스트/튜토리얼 모드

```csharp
var tutorialSetup = new InitialSetup
{
    Towers = TowerSetupDefaults.ClashRoyaleStandard(),
    InitialUnits = new List<UnitSpawnSetup>
    {
        new() { UnitId = "knight", Faction = UnitFaction.Friendly,
                Position = new Vector2(1600, 1500), Count = 3, SpawnRadius = 50 }
    },
    GameTime = new GameTimeSetup { RegularTime = 60f, MaxGameTime = 120f }
};
```

## 6. 타워 기본 위치

| 타워 | 진영 | 위치 |
|------|------|------|
| King | Friendly | MapLayout.FriendlyKingPosition |
| Princess Left | Friendly | MapLayout.FriendlyPrincessLeftPosition |
| Princess Right | Friendly | MapLayout.FriendlyPrincessRightPosition |
| King | Enemy | MapLayout.EnemyKingPosition |
| Princess Left | Enemy | MapLayout.EnemyPrincessLeftPosition |
| Princess Right | Enemy | MapLayout.EnemyPrincessRightPosition |

## 7. 향후 확장 (Phase 2)

- JSON 파일 기반 로드 (`InitialSetupLoader`)
- Tower Troops 지원
- 엘릭서 초기값 설정
