# 클래시 로얄 유닛 움직임 구현 계획

이 문서는 클래시 로얄 스타일의 유닛 움직임을 구현하기 위한 단계별 개발 계획을 기술합니다.
각 페이즈는 독립적으로 구현, 테스트, 커밋되는 것을 원칙으로 합니다.

---

## 구현 상태 요약

| Phase | 상태 | 설명 |
|-------|------|------|
| Phase 1 | ✅ 완료 | 정적 장애물 회피 (A* 경로탐색) |
| Phase 2 | ✅ 완료 | 유닛 간 충돌 회피 (AvoidanceSystem) |
| Phase 3 | ✅ 완료 | 물리적 충돌 처리 (Body Blocking) |

---

## Phase 1: 기본 토대 구축 - 정적 장애물 회피 (A\* 경로탐색 활성화)

**상태:** ✅ 완료

**목표:** 유닛이 타워, 강과 같은 정적 장애물을 인지하고 A\* 알고리즘을 통해 우회하도록 합니다.

### 구현 결과

- `UnitSimulator.Core/Pathfinding/PathfindingGrid.cs` - 그리드 기반 장애물 관리
- `UnitSimulator.Core/Pathfinding/AStarPathfinder.cs` - A* 경로탐색 알고리즘
- `UnitSimulator.Core/SimulatorCore.cs` - `ApplyStaticObstacles()` 메서드로 타워/강 장애물 적용
- `UnitSimulator.Core/Unit.cs` - `SetMovementPath()`, `TryGetNextMovementWaypoint()` 경로 추적
- `UnitSimulator.Core/SquadBehavior.cs`, `EnemyBehavior.cs` - `FindPath()` 연동

---

## Phase 2: 동적 상호작용 - 유닛 간 충돌 회피

**상태:** ✅ 완료

**목표:** 스티어링 로직을 도입하여 유닛들이 서로 부딪히지 않고 자연스럽게 비켜가도록 합니다.

### 구현 결과

- `UnitSimulator.Core/AvoidanceSystem.cs` - 예측적 회피 벡터 계산 (`PredictiveAvoidanceVector`)
- `UnitSimulator.Core/Unit.cs` - `AvoidanceThreat`, `AvoidanceTarget`, 회피 경로 관리
- `UnitSimulator.Core/Pathfinding/DynamicObstacleSystem.cs` - 동적 장애물(유닛 밀집 영역) 관리
- 레이어 기반 충돌 (Ground/Air 유닛은 같은 레이어끼리만 충돌)

---

## Phase 3: 그룹 움직임 고도화 - 물리적 충돌 처리

**상태:** ✅ 완료

**목표:** 유닛들이 서로를 통과하지 못하고, 겹쳤을 때 물리적으로 밀어내도록 하여 '몸으로 막는(Body Blocking)' 현상을 구현합니다.

### 구현 결과

- `UnitSimulator.Core/SimulatorCore.cs` - `ResolveCollisions()` 메서드 추가
  - Step() 메서드의 Phase 1.5에서 호출
  - 모든 유닛 쌍에 대해 겹침 검사
  - 같은 레이어(Ground/Air)의 유닛끼리만 충돌 해소
  - 반복적 해소로 안정적인 분리

- `UnitSimulator.Core/GameConstants.cs` - 충돌 해소 상수 추가
  - `COLLISION_RESOLUTION_ITERATIONS = 3` - 반복 횟수
  - `COLLISION_PUSH_STRENGTH = 0.8f` - 밀어내기 강도

### 충돌 해소 알고리즘

```csharp
// 겹침 발생 시
float overlap = combinedRadius - distance;
Vector2 pushDirection = SafeNormalize(unitB.Position - unitA.Position);
float pushAmount = overlap * 0.5f * COLLISION_PUSH_STRENGTH;

// 각 유닛을 반대 방향으로 밀어냄
unitA.Position -= pushDirection * pushAmount;
unitB.Position += pushDirection * pushAmount;
```

---

## 검증 방법

### Phase 1 검증
- 맵 중앙에 장애물을 배치하고 유닛이 우회하는지 확인
- 유닛의 X 좌표가 장애물을 피해 좌/우로 이동 후 복귀

### Phase 2 검증
- 두 유닛을 마주보게 배치하고 서로의 위치를 목적지로 설정
- 충돌 없이 부드러운 곡선으로 비켜가는지 확인

### Phase 3 검증
- 대형 유닛(반지름 100) 뒤에 소형 유닛(반지름 20) 배치
- 소형 유닛이 대형 유닛을 통과하지 못하고 밀려나는지 확인
- 두 유닛의 원이 겹치지 않고 유지되는지 확인

---

## 관련 파일

| 파일 | 역할 |
|------|------|
| `SimulatorCore.cs` | 시뮬레이션 루프, 충돌 해소 |
| `AvoidanceSystem.cs` | 예측적 회피 벡터 계산 |
| `PathfindingGrid.cs` | 그리드 기반 장애물 관리 |
| `AStarPathfinder.cs` | A* 경로탐색 |
| `DynamicObstacleSystem.cs` | 동적 장애물 관리 |
| `GameConstants.cs` | 충돌/회피 관련 상수 |
| `Unit.cs` | 유닛 상태 및 경로 추적 |
| `SquadBehavior.cs` | 아군 유닛 행동 |
| `EnemyBehavior.cs` | 적 유닛 행동 |
