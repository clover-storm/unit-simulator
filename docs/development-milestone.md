# Development Milestone

RTS 게임 코어 완성 및 게임 엔진 통합을 위한 개발 마일스톤 문서.

---

## 목차

1. [프로젝트 개요](#1-프로젝트-개요)
2. [현재 상태](#2-현재-상태)
3. [마일스톤 개요](#3-마일스톤-개요)
4. [Phase 1: 코어 분리 및 안정화](#4-phase-1-코어-분리-및-안정화)
5. [Phase 2: 데이터 파이프라인 정규화](#5-phase-2-데이터-파이프라인-정규화)
6. [Phase 3: 게임 엔진 선정 및 어댑터 개발](#6-phase-3-게임-엔진-선정-및-어댑터-개발)
7. [Phase 4: 게임 프로토타입](#7-phase-4-게임-프로토타입)
8. [Phase 5: 릴리즈 준비](#8-phase-5-릴리즈-준비)
9. [분산 작업 가이드](#9-분산-작업-가이드)

---

## 1. 프로젝트 개요

### 1.1 목표

- RTS 게임의 핵심 시뮬레이션 코어 완성
- 게임 엔진(Unity/Godot/Unreal 중 택1) 통합
- 데이터 드리븐 밸런싱 시스템 구축
- 최종 게임 릴리즈

### 1.2 핵심 원칙

| 원칙 | 설명 |
|------|------|
| **Deterministic** | 동일 입력 → 동일 결과. 리플레이/네트워크 동기화 필수 조건 |
| **Engine Agnostic** | 코어는 렌더링/입력/사운드 의존성 없음 |
| **Data-Driven** | 코드 수정 없이 밸런스 조정 가능 |
| **Command Pattern** | 모든 게임 액션은 직렬화 가능한 커맨드 |
| **Testable** | 모든 모듈은 독립적으로 테스트 가능 |

---

## 2. 현재 상태

### 2.1 솔루션 구조

```
unit-simulator/
├── UnitMove/              # RTS 시뮬레이션 코어 + 개발 서버
│   ├── SimulatorCore.cs   # 핵심 시뮬레이션 엔진
│   ├── Unit.cs            # 유닛 상태/행동
│   ├── SquadBehavior.cs   # 아군 AI
│   ├── EnemyBehavior.cs   # 적군 AI
│   ├── WaveManager.cs     # 웨이브 시스템
│   ├── WebSocketServer.cs # 개발용 서버
│   └── Session*.cs        # 멀티세션 지원
│
├── ReferenceModels/       # 데이터 드리븐 모듈
│   └── (Google Sheets 연동)
│
├── gui-viewer/            # React 기반 개발 도구
│   └── (시뮬레이션 시각화/디버깅)
│
├── dev-tool/              # 개발 인프라
└── docs/                  # 문서
```

### 2.2 완료된 기능

- [x] 유닛 이동/전투 시뮬레이션
- [x] 웨이브 기반 적 스폰
- [x] 프레임 기반 상태 관리
- [x] WebSocket 실시간 통신
- [x] 멀티세션 지원
- [x] 세션별 output 격리
- [x] GUI 시각화 도구

### 2.3 미완료/개선 필요

- [ ] 코어와 인프라 코드 분리
- [ ] 렌더링 의존성 제거 (ImageSharp)
- [ ] 데이터 스키마 표준화
- [ ] 유닛 테스트
- [ ] 게임 엔진 통합

---

## 3. 마일스톤 개요

```
Phase 1          Phase 2          Phase 3          Phase 4          Phase 5
코어 분리    →   데이터 정규화  →  엔진 통합    →   프로토타입   →   릴리즈
(2-3주)          (1-2주)          (3-4주)          (4-6주)          (2-3주)
   │                │                │                │                │
   ├─ M1.1          ├─ M2.1          ├─ M3.1          ├─ M4.1          ├─ M5.1
   ├─ M1.2          ├─ M2.2          ├─ M3.2          ├─ M4.2          ├─ M5.2
   ├─ M1.3          └─ M2.3          ├─ M3.3          ├─ M4.3          └─ M5.3
   └─ M1.4                           └─ M3.4          └─ M4.4
```

---

## 4. Phase 1: 코어 분리 및 안정화

**목표**: 게임 엔진 독립적인 순수 시뮬레이션 라이브러리 추출

### M1.1: 프로젝트 구조 재편

**담당**: 인프라/아키텍처

**작업 내용**:

```
[현재]                              [목표]
UnitMove/                           UnitSimulator.Core/      ← 순수 코어
├── SimulatorCore.cs                ├── Simulation/
├── Unit.cs                         │   ├── SimulatorCore.cs
├── WebSocketServer.cs              │   ├── FrameData.cs
└── ...                             │   └── ISimulatorCallbacks.cs
                                    ├── Units/
                                    │   ├── Unit.cs
                                    │   ├── UnitFaction.cs
                                    │   └── UnitRole.cs
                                    ├── Behaviors/
                                    │   ├── IBehavior.cs
                                    │   ├── SquadBehavior.cs
                                    │   └── EnemyBehavior.cs
                                    ├── Systems/
                                    │   ├── WaveManager.cs
                                    │   └── CombatSystem.cs
                                    └── Data/
                                        ├── IDataProvider.cs
                                        └── GameConfig.cs

                                    UnitSimulator.Server/    ← 개발 도구
                                    ├── WebSocketServer.cs
                                    ├── SessionManager.cs
                                    └── Program.cs
```

**입력**: 현재 UnitMove 프로젝트
**출력**: 분리된 두 개의 프로젝트 (Core, Server)

**완료 조건**:
- [ ] UnitSimulator.Core는 System.* 외 외부 의존성 없음
- [ ] UnitSimulator.Server는 Core를 참조
- [ ] 기존 기능 동작 확인

---

### M1.2: 렌더링 의존성 제거

**담당**: 코어 개발

**작업 내용**:

현재 `Renderer` 클래스가 `SixLabors.ImageSharp`에 의존. 이를 코어에서 완전히 분리.

```csharp
// 제거 대상 (Core에서)
using SixLabors.ImageSharp;
using SixLabors.Fonts;

// Core는 오직 데이터만 출력
public interface IFrameRenderer
{
    void RenderFrame(FrameData frameData);
}

// Server에서 구현
public class ImageRenderer : IFrameRenderer { ... }
public class NullRenderer : IFrameRenderer { ... }  // 헤드리스 모드
```

**입력**: M1.1 완료된 프로젝트
**출력**: 렌더링 로직이 Server로 이동된 프로젝트

**완료 조건**:
- [ ] Core 프로젝트에 ImageSharp 참조 없음
- [ ] Core는 .NET Standard 2.1 호환
- [ ] 렌더링은 선택적 기능

---

### M1.3: 인터페이스 계약 정의

**담당**: 아키텍처 설계

**작업 내용**:

게임 엔진 어댑터가 사용할 공개 API 정의.

```csharp
namespace UnitSimulator.Core.Contracts
{
    /// <summary>
    /// 시뮬레이션 엔진 메인 인터페이스
    /// </summary>
    public interface ISimulation
    {
        // Lifecycle
        void Initialize(GameConfig config);
        void Reset();
        void Dispose();

        // Simulation Control
        FrameData Step();
        FrameData GetCurrentState();
        void LoadState(FrameData state);

        // Properties
        int CurrentFrame { get; }
        bool IsComplete { get; }
        SimulationStatus Status { get; }
    }

    /// <summary>
    /// 유닛 제어 인터페이스
    /// </summary>
    public interface IUnitController
    {
        void MoveUnit(int unitId, UnitFaction faction, Vector2 destination);
        void SetUnitHealth(int unitId, UnitFaction faction, int health);
        void KillUnit(int unitId, UnitFaction faction);
        void ReviveUnit(int unitId, UnitFaction faction, int health);
        Unit SpawnUnit(Vector2 position, UnitRole role, UnitFaction faction);
    }

    /// <summary>
    /// 게임 데이터 제공자
    /// </summary>
    public interface IDataProvider
    {
        UnitStats GetUnitStats(UnitRole role, UnitFaction faction);
        WaveDefinition GetWaveDefinition(int waveNumber);
        GameBalance GetGameBalance();
    }

    /// <summary>
    /// 시뮬레이션 이벤트 수신자
    /// </summary>
    public interface ISimulationObserver
    {
        void OnFrameAdvanced(FrameData frameData);
        void OnUnitSpawned(Unit unit);
        void OnUnitDied(Unit unit, Unit killer);
        void OnUnitDamaged(Unit unit, int damage, Unit attacker);
        void OnWaveStarted(int waveNumber);
        void OnSimulationComplete(string reason);
    }
}
```

**입력**: 기존 코드 분석
**출력**: `Contracts/` 디렉토리에 인터페이스 정의

**완료 조건**:
- [ ] 모든 공개 API가 인터페이스로 정의됨
- [ ] XML 문서 주석 완비
- [ ] 버전 관리 고려 (향후 확장성)

---

### M1.4: 유닛 테스트 구축

**담당**: QA/테스트

**작업 내용**:

```
UnitSimulator.Core.Tests/
├── Simulation/
│   ├── SimulatorCoreTests.cs
│   ├── FrameDataTests.cs
│   └── DeterminismTests.cs      ← 동일 입력 → 동일 결과 검증
├── Units/
│   ├── UnitTests.cs
│   ├── CombatTests.cs
│   └── MovementTests.cs
├── Behaviors/
│   ├── SquadBehaviorTests.cs
│   └── EnemyBehaviorTests.cs
└── Integration/
    └── FullSimulationTests.cs
```

**핵심 테스트 케이스**:

```csharp
[Test]
public void Simulation_SameInput_ProducesSameOutput()
{
    // Determinism 검증
    var sim1 = new SimulatorCore();
    var sim2 = new SimulatorCore();

    sim1.Initialize(config);
    sim2.Initialize(config);

    for (int i = 0; i < 100; i++)
    {
        var frame1 = sim1.Step();
        var frame2 = sim2.Step();
        Assert.AreEqual(frame1.ToJson(), frame2.ToJson());
    }
}
```

**입력**: M1.3 완료된 인터페이스
**출력**: 테스트 프로젝트 및 80%+ 커버리지

**완료 조건**:
- [ ] 모든 공개 API 테스트 커버
- [ ] Determinism 테스트 통과
- [ ] CI 파이프라인에서 자동 실행

---

## 5. Phase 2: 데이터 파이프라인 정규화

**목표**: Google Sheets → 게임 데이터 자동화 파이프라인 구축

### M2.1: 데이터 스키마 표준화

**담당**: 데이터 아키텍처

**작업 내용**:

```
data/
├── schemas/                    # JSON Schema 정의
│   ├── unit-stats.schema.json
│   ├── wave-definition.schema.json
│   ├── game-balance.schema.json
│   └── localization.schema.json
│
├── raw/                        # Google Sheets에서 다운로드된 원본
│   └── (자동 생성)
│
├── processed/                  # 변환된 게임 데이터
│   ├── units.json
│   ├── waves.json
│   ├── balance.json
│   └── strings/
│       ├── en.json
│       └── ko.json
│
└── validation/                 # 검증 결과
    └── report.json
```

**스키마 예시**:

```json
// unit-stats.schema.json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "properties": {
    "unitId": { "type": "string" },
    "role": { "enum": ["Melee", "Ranged", "Tank", "Support"] },
    "faction": { "enum": ["Friendly", "Enemy"] },
    "stats": {
      "type": "object",
      "properties": {
        "hp": { "type": "integer", "minimum": 1 },
        "damage": { "type": "integer", "minimum": 0 },
        "attackSpeed": { "type": "number", "minimum": 0 },
        "moveSpeed": { "type": "number", "minimum": 0 },
        "attackRange": { "type": "number", "minimum": 0 }
      },
      "required": ["hp", "damage", "attackSpeed", "moveSpeed", "attackRange"]
    }
  },
  "required": ["unitId", "role", "faction", "stats"]
}
```

**입력**: 현재 ReferenceModels 구조 분석
**출력**: 표준화된 스키마 및 샘플 데이터

**완료 조건**:
- [ ] 모든 게임 데이터 타입에 대한 스키마 정의
- [ ] 스키마 검증 통과하는 샘플 데이터
- [ ] ReferenceModels가 스키마 기반으로 동작

---

### M2.2: 데이터 변환 파이프라인

**담당**: 빌드/인프라

**작업 내용**:

```
npm run data:sync     # Google Sheets → raw/
npm run data:convert  # raw/ → processed/
npm run data:validate # 스키마 검증
npm run data:build    # 전체 파이프라인
```

**파이프라인 구조**:

```
┌─────────────────┐     ┌──────────────┐     ┌───────────────┐
│  Google Sheets  │────►│ ReferenceModels│────►│ processed/*.json│
│  (원본 데이터)   │     │ (C# 변환기)   │     │ (게임 데이터)  │
└─────────────────┘     └──────────────┘     └───────────────┘
                                                     │
                              ┌──────────────────────┼──────────────────────┐
                              ▼                      ▼                      ▼
                        UnitSimulator.Core    Unity/Godot/Unreal    gui-viewer
```

**입력**: M2.1 스키마
**출력**: 자동화된 데이터 파이프라인

**완료 조건**:
- [ ] 단일 명령으로 전체 파이프라인 실행
- [ ] 스키마 검증 실패 시 빌드 중단
- [ ] 변환 결과 diff 출력 (변경사항 추적)

---

### M2.3: 런타임 데이터 로더

**담당**: 코어 개발

**작업 내용**:

```csharp
namespace UnitSimulator.Core.Data
{
    public class JsonDataProvider : IDataProvider
    {
        private readonly Dictionary<string, UnitStats> _unitStats;
        private readonly List<WaveDefinition> _waves;
        private readonly GameBalance _balance;

        public JsonDataProvider(string dataPath)
        {
            _unitStats = LoadJson<Dictionary<string, UnitStats>>(
                Path.Combine(dataPath, "units.json"));
            _waves = LoadJson<List<WaveDefinition>>(
                Path.Combine(dataPath, "waves.json"));
            _balance = LoadJson<GameBalance>(
                Path.Combine(dataPath, "balance.json"));
        }

        public UnitStats GetUnitStats(UnitRole role, UnitFaction faction)
            => _unitStats[$"{faction}_{role}"];

        public WaveDefinition GetWaveDefinition(int waveNumber)
            => _waves[waveNumber - 1];

        public GameBalance GetGameBalance()
            => _balance;
    }
}
```

**입력**: M2.2 처리된 데이터
**출력**: 런타임 데이터 로딩 구현

**완료 조건**:
- [ ] Core가 외부 JSON에서 데이터 로드
- [ ] 하드코딩된 상수값 제거
- [ ] 핫 리로드 지원 (개발 모드)

---

## 6. Phase 3: 게임 엔진 선정 및 어댑터 개발

**목표**: 최적 게임 엔진 선정 후 통합 어댑터 개발

### M3.1: 게임 엔진 평가

**담당**: 기술 리서치

**평가 기준**:

| 기준 | 가중치 | Unity | Godot | Unreal |
|------|--------|-------|-------|--------|
| C# 통합 용이성 | 30% | ★★★★★ | ★★★★☆ | ★★☆☆☆ |
| 2D RTS 지원 | 25% | ★★★★☆ | ★★★★★ | ★★★☆☆ |
| 라이선스 비용 | 15% | ★★★☆☆ | ★★★★★ | ★★★☆☆ |
| 빌드 타겟 | 15% | ★★★★★ | ★★★★☆ | ★★★★☆ |
| 팀 숙련도 | 15% | ? | ? | ? |

**평가 항목**:

1. **기술 호환성**
   - .NET 버전 호환
   - 시뮬레이션 코어 호출 방식
   - 빌드 파이프라인 통합

2. **개발 효율성**
   - 에디터 기능
   - 디버깅 도구
   - 에셋 파이프라인

3. **배포 및 유지보수**
   - 타겟 플랫폼
   - 업데이트 주기
   - 커뮤니티 지원

**입력**: 각 엔진별 프로토타입 테스트
**출력**: 엔진 선정 보고서 및 최종 결정

**완료 조건**:
- [ ] 3개 엔진 프로토타입 구현 (최소 기능)
- [ ] 비교 평가 문서 작성
- [ ] 최종 엔진 선정

---

### M3.2: 엔진 어댑터 구조 설계

**담당**: 아키텍처 설계

**작업 내용**:

```csharp
namespace UnitSimulator.EngineAdapter
{
    /// <summary>
    /// 게임 엔진과 시뮬레이션 코어를 연결하는 브릿지
    /// </summary>
    public abstract class SimulationBridge : ISimulationObserver
    {
        protected ISimulation Simulation { get; }
        protected IUnitController UnitController { get; }

        // 엔진별 구현 필요
        protected abstract void OnFrameAdvanced(FrameData frame);
        protected abstract void SpawnVisualUnit(Unit unit);
        protected abstract void UpdateVisualUnit(Unit unit);
        protected abstract void DestroyVisualUnit(Unit unit);

        // 공통 로직
        public void Tick(float deltaTime)
        {
            // 고정 타임스텝으로 시뮬레이션 실행
            _accumulator += deltaTime;
            while (_accumulator >= FixedTimeStep)
            {
                var frame = Simulation.Step();
                SyncVisuals(frame);
                _accumulator -= FixedTimeStep;
            }
        }
    }

    // Unity 구현 예시
    public class UnitySimulationBridge : SimulationBridge
    {
        private Dictionary<int, GameObject> _unitObjects;

        protected override void SpawnVisualUnit(Unit unit)
        {
            var prefab = GetUnitPrefab(unit.Role, unit.Faction);
            var go = Instantiate(prefab, unit.Position.ToVector3(), Quaternion.identity);
            _unitObjects[unit.Id] = go;
        }
    }
}
```

**입력**: M1.3 인터페이스, M3.1 선정된 엔진
**출력**: 어댑터 아키텍처 문서 및 기본 구현

**완료 조건**:
- [ ] 엔진별 어댑터 인터페이스 정의
- [ ] 시뮬레이션 ↔ 렌더링 동기화 구현
- [ ] 입력 처리 파이프라인 구현

---

### M3.3: 엔진 프로젝트 템플릿

**담당**: 엔진 개발

**작업 내용** (Unity 예시):

```
unit-simulator-unity/
├── Assets/
│   ├── Plugins/
│   │   └── UnitSimulator.Core.dll
│   │
│   ├── Scripts/
│   │   ├── Bridge/
│   │   │   ├── UnitySimulationBridge.cs
│   │   │   └── UnityDataProvider.cs
│   │   ├── Units/
│   │   │   ├── UnitVisual.cs
│   │   │   └── UnitAnimator.cs
│   │   ├── UI/
│   │   │   ├── GameHUD.cs
│   │   │   └── UnitSelectionUI.cs
│   │   └── Input/
│   │       └── RTSInputHandler.cs
│   │
│   ├── Prefabs/
│   │   ├── Units/
│   │   │   ├── FriendlyMelee.prefab
│   │   │   └── EnemyRanged.prefab
│   │   └── UI/
│   │
│   ├── Scenes/
│   │   ├── MainMenu.unity
│   │   ├── Gameplay.unity
│   │   └── DevTools.unity
│   │
│   └── Resources/
│       └── GameData/           ← processed/*.json 복사
│
├── Packages/
│   └── manifest.json
│
└── ProjectSettings/
```

**입력**: M3.2 어댑터 설계
**출력**: 실행 가능한 엔진 프로젝트

**완료 조건**:
- [ ] 시뮬레이션 실행 및 시각화
- [ ] 기본 RTS 조작 (선택, 이동 명령)
- [ ] 개발자 도구 (디버그 뷰)

---

### M3.4: 개발 워크플로우 통합

**담당**: 빌드/인프라

**작업 내용**:

```
npm run dev              # 개발 서버 (현재)
npm run dev:engine       # 게임 엔진 프로젝트 실행
npm run build:all        # 코어 + 엔진 전체 빌드
npm run sync:data        # 데이터 → 엔진 프로젝트 동기화
```

**CI/CD 파이프라인**:

```yaml
# .github/workflows/build.yml
jobs:
  build-core:
    - dotnet build UnitSimulator.Core
    - dotnet test UnitSimulator.Core.Tests

  build-engine:
    needs: build-core
    - # Unity/Godot CLI 빌드

  deploy:
    needs: build-engine
    - # 빌드 아티팩트 배포
```

**입력**: M3.3 엔진 프로젝트
**출력**: 통합 빌드 스크립트 및 CI 설정

**완료 조건**:
- [ ] 단일 명령으로 전체 빌드
- [ ] 코어 변경 시 엔진 프로젝트 자동 반영
- [ ] 자동화된 빌드 파이프라인

---

## 7. Phase 4: 게임 프로토타입

**목표**: 플레이 가능한 게임 프로토타입 완성

### M4.1: 핵심 게임플레이 루프

**담당**: 게임 개발

**구현 항목**:

- [ ] 메인 메뉴 → 게임 → 결과 화면 흐름
- [ ] 유닛 선택 및 이동 명령
- [ ] 웨이브 클리어 조건
- [ ] 승리/패배 조건
- [ ] 기본 HUD (유닛 HP, 웨이브 정보)

**입력**: M3.3 엔진 프로젝트
**출력**: 핵심 루프 플레이 가능

---

### M4.2: 아트 에셋 통합

**담당**: 아트/엔진

**구현 항목**:

- [ ] 유닛 스프라이트/모델
- [ ] 애니메이션 (이동, 공격, 사망)
- [ ] 이펙트 (피격, 스킬)
- [ ] UI 에셋
- [ ] 사운드/BGM

**입력**: 아트 에셋 (외부 제작)
**출력**: 에셋 통합된 게임

---

### M4.3: 게임 시스템 확장

**담당**: 게임 개발

**구현 항목**:

- [ ] 유닛 능력/스킬 시스템
- [ ] 업그레이드 시스템
- [ ] 난이도 조절
- [ ] 세이브/로드
- [ ] 설정 (사운드, 그래픽)

**입력**: M4.1 핵심 루프
**출력**: 확장된 게임 시스템

---

### M4.4: 플레이테스트 및 밸런싱

**담당**: QA/기획

**작업 항목**:

- [ ] 내부 플레이테스트
- [ ] 밸런스 데이터 조정 (Google Sheets)
- [ ] 버그 리포트 및 수정
- [ ] UX 개선

**입력**: M4.3 완성된 게임
**출력**: 밸런싱된 게임

---

## 8. Phase 5: 릴리즈 준비

### M5.1: 최적화

- [ ] 프로파일링 및 병목 제거
- [ ] 메모리 최적화
- [ ] 로딩 시간 단축
- [ ] 배터리/발열 최적화 (모바일)

### M5.2: 플랫폼별 빌드

- [ ] 타겟 플랫폼 빌드 설정
- [ ] 플랫폼별 테스트
- [ ] 스토어 요구사항 충족

### M5.3: 출시 준비

- [ ] 스토어 페이지 준비
- [ ] 마케팅 자료
- [ ] 출시 체크리스트
- [ ] 모니터링/분석 설정

---

## 9. 분산 작업 가이드

### 9.1 작업 격리 원칙

각 마일스톤은 **독립적으로 작업 가능**하도록 설계되었습니다.

```
┌─────────────────────────────────────────────────────────────┐
│                      작업 의존성 그래프                       │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  M1.1 ──► M1.2 ──► M1.3 ──► M1.4                            │
│    │                 │                                       │
│    │                 ▼                                       │
│    │              M2.1 ──► M2.2 ──► M2.3                    │
│    │                                  │                      │
│    ▼                                  ▼                      │
│  M3.1 ─────────────────────────────► M3.2 ──► M3.3 ──► M3.4│
│                                              │              │
│                                              ▼              │
│                                           M4.1 ──► ...      │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

### 9.2 작업 분배 단위

| 작업 단위 | 담당 모델 | 필요 컨텍스트 | 산출물 |
|-----------|-----------|---------------|--------|
| M1.1 구조 재편 | 아키텍처 전문 | 전체 코드베이스 | 분리된 프로젝트 |
| M1.2 의존성 제거 | 코어 개발 | UnitMove/ | 정리된 Core |
| M1.3 인터페이스 | 아키텍처 설계 | M1.1 결과 | 인터페이스 정의 |
| M1.4 테스트 | QA 전문 | M1.3 인터페이스 | 테스트 코드 |
| M2.* 데이터 | 데이터 엔지니어 | ReferenceModels/ | 데이터 파이프라인 |
| M3.* 엔진 | 게임 엔진 전문 | Core + 선택된 엔진 | 엔진 통합 |
| M4.* 게임 | 게임 개발 | 엔진 프로젝트 | 게임 콘텐츠 |

### 9.3 작업 핸드오프 체크리스트

**작업 시작 전**:
```markdown
## 작업 컨텍스트
- 마일스톤: M1.2
- 선행 작업: M1.1 (완료)
- 필요 파일: UnitSimulator.Core/
- 참조 문서: docs/development-milestone.md

## 완료 조건
- [ ] 조건 1
- [ ] 조건 2
- [ ] 조건 3

## 제약 사항
- Core는 System.* 외 외부 참조 금지
- 기존 테스트 통과 필수
```

**작업 완료 후**:
```markdown
## 작업 결과
- 변경된 파일: [목록]
- 추가된 파일: [목록]
- 삭제된 파일: [목록]

## 다음 작업자 참고
- [특이사항]
- [주의사항]

## 테스트 결과
- dotnet test: PASS
- 수동 테스트: [결과]
```

### 9.4 병렬 작업 가능 조합

```
동시 진행 가능:
├── M1.3 (인터페이스) + M2.1 (스키마)     ← 서로 독립적
├── M1.4 (테스트) + M2.2 (파이프라인)     ← M1.3, M2.1 완료 후
└── M3.3 (엔진) + M2.3 (데이터 로더)      ← 각각 다른 영역

순차 진행 필요:
├── M1.1 → M1.2 → M1.3                    ← 코어 분리 체인
├── M2.1 → M2.2 → M2.3                    ← 데이터 체인
└── M3.1 → M3.2 → M3.3 → M3.4             ← 엔진 체인
```

---

## 변경 이력

| 날짜 | 버전 | 변경 내용 |
|------|------|-----------|
| 2024-12-21 | 1.0 | 초안 작성 |
