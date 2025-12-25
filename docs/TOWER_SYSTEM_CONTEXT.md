# Tower System Implementation Context

이 문서는 Tower System 구현의 중간 상태를 기록합니다.
컨텍스트 초기화 후에도 작업을 재개할 수 있도록 필요한 정보를 포함합니다.

## 작업 요약

클래시로얄 스타일의 타워 기반 승패 시스템을 구현 중입니다.

## 완료된 Phase

### Phase A: Tower 기본 클래스 ✅
- `UnitSimulator.Core/Towers/TowerType.cs` - Princess, King enum
- `UnitSimulator.Core/Towers/Tower.cs` - 타워 클래스 (HP, 공격, 타겟팅)
- `UnitSimulator.Core/Towers/TowerStats.cs` - Level 11 스탯, 팩토리 메서드

### Phase B: Game State ✅
- `UnitSimulator.Core/GameState/GameResult.cs` - GameResult, WinCondition enum
- `UnitSimulator.Core/GameState/MapLayout.cs` - 맵 크기, 타워 위치, 강/다리
- `UnitSimulator.Core/GameState/GameSession.cs` - 타워 관리, 크라운, 게임 상태

### Phase C: Tower Combat ✅
- `UnitSimulator.Core/Towers/TowerBehavior.cs` - 타워 업데이트, 타겟팅, 공격
- `UnitSimulator.Core/Combat/FrameEvents.cs` - TowerDamageEvent, DamageToTowerEvent 추가

## 남은 Phase

### Phase D: Unit 타워 타겟팅 (다음 작업)
**수정 파일:**
- `UnitSimulator.Core/Unit.cs` - TargetPriority, TargetTower 필드 추가
- `UnitSimulator.Core/SquadBehavior.cs` - 타워 타겟팅 로직 추가
- `UnitSimulator.Core/EnemyBehavior.cs` - 타워 타겟팅 로직 추가

**신규 파일:**
- `UnitSimulator.Core/Targeting/TowerTargetingRules.cs`

**구현 내용:**
```csharp
public enum TargetPriority
{
    Nearest,    // 가장 가까운 적 (기본)
    Buildings   // 타워 우선 (Giant, Golem, Hog Rider)
}

// Unit.cs에 추가
public TargetPriority TargetPriority { get; init; } = TargetPriority.Nearest;
public Tower? TargetTower { get; set; }
public bool CanAttackTower(Tower tower) { ... }
```

### Phase E: WinConditionEvaluator
**신규 파일:**
- `UnitSimulator.Core/GameState/WinConditionEvaluator.cs`

**구현 내용:**
- King Tower 파괴 → 즉시 승패
- 정규 시간(180초) 종료 → 크라운 비교
- 연장전(300초) 종료 → HP 비율 비교

### Phase F: SimulatorCore 통합
**수정 파일:**
- `UnitSimulator.Core/SimulatorCore.cs`

**구현 내용:**
- GameSession 필드 추가
- TowerBehavior 필드 추가
- Step()에서 타워 업데이트 호출
- ApplyTowerDamageEvents(), ApplyDamageToTowers() 추가

### Phase G: FrameData 직렬화
**수정 파일:**
- `UnitSimulator.Core/FrameData.cs`

**구현 내용:**
```csharp
public List<TowerStateData> FriendlyTowers { get; init; }
public List<TowerStateData> EnemyTowers { get; init; }
public float ElapsedTime { get; init; }
public int FriendlyCrowns { get; init; }
public int EnemyCrowns { get; init; }
public GameResult GameResult { get; init; }
public bool IsOvertime { get; init; }
```

### Phase H: TerrainSystem (강/다리)
**신규 파일:**
- `UnitSimulator.Core/Terrain/TerrainSystem.cs`

**수정 파일:**
- `UnitSimulator.Core/SquadBehavior.cs` - CanMoveTo 체크 추가

## 관련 스펙 문서

- `docs/unit-system-spec.md` - Section 12 (Tower System), Section 13 (River & Bridge)
- `docs/simulation-spec.md` - Section 6 (맵 및 타워 시스템)

## 구현 계획 파일

- `/Users/jshstorm/.claude/plans/synchronous-exploring-ember.md`

## 커밋 이력

- `aa27e91` - feat(core): add Tower System foundation (Phase A-C)
- `f152862` - fix(server): add FrameEvents parameter to legacy simulation
- `85691a3` - docs: add Tower System and Win Conditions spec
- `ff29f40` - feat(core): add Reference system for data-driven unit loading

## 재개 방법

1. 이 문서를 읽어 현재 상태 파악
2. `docs/unit-system-spec.md` Section 12, 13 참조
3. Phase D부터 순차적으로 구현
4. 각 Phase 완료 후 빌드 및 테스트 확인

## 빌드 명령어

```bash
dotnet build UnitSimulator.sln
dotnet test UnitSimulator.Core.Tests
```

## 현재 테스트 상태

- 39개 테스트 통과
- 경고: CS8602 (null reference) - SimulatorCore.cs:454
- 경고: CS0618 (obsolete UnitRegistry) - 테스트에서 예상됨
