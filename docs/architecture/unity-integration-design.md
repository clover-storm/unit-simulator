# Unity Integration Design

> **문서 유형**: Architecture Decision Record (ADR)
> **상태**: Draft
> **작성일**: 2026-01-20
> **관련 Phase**: Phase 3 (게임 엔진 선정 및 어댑터 개발)

---

## 목차

1. [개요](#1-개요)
2. [아키텍처 개요](#2-아키텍처-개요)
3. [공유 Contracts 모듈](#3-공유-contracts-모듈)
4. [RefId 시스템](#4-refid-시스템)
5. [EntityViewData 설계](#5-entityviewdata-설계)
6. [이벤트 시스템](#6-이벤트-시스템)
7. [프레임 동기화 전략](#7-프레임-동기화-전략)
8. [Reference 스키마 확장](#8-reference-스키마-확장)
9. [Unity 측 구현 가이드](#9-unity-측-구현-가이드)
10. [구현 우선순위](#10-구현-우선순위)

---

## 1. 개요

### 1.1 목적

UnitSimulator.Core와 Unity 게임 엔진 간의 통합을 위한 인터페이스, 데이터 모델, 동기화 전략을 정의합니다.

### 1.2 설계 원칙

| 원칙 | 설명 |
|------|------|
| **Engine Agnostic Core** | Core는 Unity에 대한 의존성 없이 유지 |
| **Shared Contracts** | Core와 Unity가 공유하는 계약 모듈 |
| **RefId 기반 참조** | 문자열 ID 대신 타입 안전한 RefId 사용 |
| **Value 기반 동기화** | 객체 참조 대신 값 복사 (Pooled) |
| **Zero GC** | 런타임 중 GC 할당 최소화 |

### 1.3 핵심 결정 사항

1. **공유 모듈**: `UnitSimulator.Contracts` (netstandard2.1)
2. **동기화 방식**: Pooled Value 기반 스냅샷
3. **이벤트 관리**: Ring Buffer 기반
4. **Reference 데이터**: Visual 정보 확장 (effects, sounds, animations)

---

## 2. 아키텍처 개요

### 2.1 계층 구조

```
┌─────────────────────────────────────────────────────────────────┐
│                       솔루션 구조                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  UnitSimulator.Contracts/        ◄── 공유 모듈 (신규)            │
│  ├── Refs/                       # RefId 타입들                  │
│  ├── Views/                      # EntityViewData 등             │
│  ├── Events/                     # 게임 이벤트                   │
│  └── Enums/                      # 공유 열거형                   │
│                                                                  │
│  ReferenceModels/                ◄── 참조 데이터 (기존)          │
│  ├── References Contracts                                        │
│  └── JSON 로딩/파싱                                              │
│                                                                  │
│  UnitSimulator.Core/             ◄── 시뮬레이션 로직             │
│  ├── References Contracts                                        │
│  └── References ReferenceModels                                  │
│                                                                  │
│  Unity Project/                  ◄── 게임 클라이언트             │
│  ├── References Contracts (DLL)                                  │
│  └── References ReferenceModels (DLL, optional)                  │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 데이터 흐름

```
┌─────────────────────────────────────────────────────────────────┐
│                        Every Frame                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│  1. Core.Step()                                                  │
│     │                                                            │
│     ├──► FrameSnapshot (Pooled)                                  │
│     │    ├── EntityViewData[] (struct 배열)                      │
│     │    └── GameEvent[] (Ring Buffer)                           │
│     │                                                            │
│     └──► Unity에 전달 (Span<T>, 복사 없음)                        │
│                                                                  │
│  2. Unity.FixedUpdate()                                          │
│     ├── Entities 순회 → Visual Transform/State 동기화            │
│     └── Events 순회 → VFX/SFX/Animation 트리거                   │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## 3. 공유 Contracts 모듈

### 3.1 프로젝트 설정

```xml
<!-- UnitSimulator.Contracts.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>  <!-- Unity 호환 -->
    <Nullable>enable</Nullable>
    <LangVersion>9.0</LangVersion>
  </PropertyGroup>
</Project>
```

### 3.2 디렉토리 구조

```
UnitSimulator.Contracts/
├── Refs/
│   ├── RefId.cs              # 기본 RefId 추상 클래스
│   ├── UnitRefId.cs
│   ├── SkillRefId.cs
│   ├── TowerRefId.cs
│   ├── EffectRefId.cs
│   ├── ProjectileRefId.cs
│   ├── AnimRefId.cs
│   └── SoundRefId.cs
├── Views/
│   ├── EntityViewData.cs     # 엔티티 뷰 구조체
│   ├── EntityFlags.cs
│   └── FrameSnapshot.cs
├── Events/
│   ├── IGameEvent.cs
│   ├── EntitySpawnEvent.cs
│   ├── EntityDeathEvent.cs
│   ├── AttackEvent.cs
│   ├── HitEvent.cs
│   ├── ProjectileSpawnEvent.cs
│   ├── SkillActivatedEvent.cs
│   └── AreaEffectEvent.cs
└── Enums/
    ├── EntityType.cs
    ├── Faction.cs
    ├── MotionState.cs
    ├── AttackType.cs
    ├── DamageType.cs
    └── DeathType.cs
```

---

## 4. RefId 시스템

### 4.1 설계 목적

- **타입 안전성**: 컴파일 타임에 ID 종류 검증
- **Reference 데이터 연동**: JSON의 키와 직접 매핑
- **Unity 호환**: 직렬화/역직렬화 지원

### 4.2 기본 구현

```csharp
namespace UnitSimulator.Contracts;

/// <summary>
/// 타입 안전한 레퍼런스 ID 기본 클래스
/// </summary>
public abstract record RefId
{
    public string Value { get; }

    protected RefId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("RefId cannot be empty", nameof(value));
        Value = value.ToLowerInvariant();
    }

    public override string ToString() => Value;

    /// <summary>
    /// 빠른 비교를 위한 해시 (Dictionary 키 등)
    /// </summary>
    public int Hash => Value.GetHashCode();

    public static implicit operator string(RefId id) => id.Value;
}
```

### 4.3 구체적인 RefId 타입들

| 타입 | 용도 | 예시 |
|------|------|------|
| `UnitRefId` | 유닛 정의 | "golem", "skeleton" |
| `SkillRefId` | 스킬 정의 | "fireball", "golem_death_spawn" |
| `TowerRefId` | 타워 정의 | "cannon_tower", "archer_tower" |
| `EffectRefId` | VFX 정의 | "fx_explosion_fire", "fx_heal_aura" |
| `ProjectileRefId` | 투사체 정의 | "arrow_normal", "fireball_proj" |
| `AnimRefId` | 애니메이션 클립 | "attack_slash", "walk_normal" |
| `SoundRefId` | 사운드 클립 | "sfx_hit_metal", "sfx_death" |

```csharp
public sealed record UnitRefId : RefId
{
    public UnitRefId(string value) : base(value) { }
    public static UnitRefId From(string value) => new(value);
}

public sealed record EffectRefId : RefId
{
    public EffectRefId(string value) : base(value) { }
    public static EffectRefId From(string value) => new(value);
}
// ... 나머지 타입들
```

---

## 5. EntityViewData 설계

### 5.1 구조체 정의 (Zero GC)

```csharp
namespace UnitSimulator.Contracts.Views;

/// <summary>
/// Unity에서 렌더링할 엔티티 뷰 데이터
/// struct로 정의하여 힙 할당 방지
/// </summary>
public struct EntityViewData
{
    // ═══════════════════════════════════════════════════════════
    // 식별자 (16 bytes)
    // ═══════════════════════════════════════════════════════════

    /// <summary>런타임 인스턴스 ID</summary>
    public int InstanceId;

    /// <summary>RefId의 해시값 (문자열 비교 회피)</summary>
    public int RefIdHash;

    /// <summary>엔티티 유형</summary>
    public EntityType Type;

    /// <summary>팀/진영</summary>
    public Faction Faction;

    // ═══════════════════════════════════════════════════════════
    // 트랜스폼 (20 bytes)
    // ═══════════════════════════════════════════════════════════

    public float PosX;
    public float PosY;
    public float PosZ;          // 공중 유닛 높이
    public float RotationY;     // Y축 회전 (degrees)
    public float Scale;

    // ═══════════════════════════════════════════════════════════
    // 상태 (12 bytes)
    // ═══════════════════════════════════════════════════════════

    public float HealthRatio;   // 0.0 ~ 1.0
    public float ShieldRatio;   // 0.0 ~ 1.0
    public MotionState Motion;

    // ═══════════════════════════════════════════════════════════
    // 애니메이션 (8 bytes)
    // ═══════════════════════════════════════════════════════════

    public int AnimHash;        // 현재 애니메이션 해시
    public float AnimProgress;  // 0.0 ~ 1.0

    // ═══════════════════════════════════════════════════════════
    // 플래그 (4 bytes)
    // ═══════════════════════════════════════════════════════════

    public EntityFlags Flags;
}

// 총 크기: ~64 bytes per entity
```

### 5.2 EntityFlags (비트 플래그)

```csharp
[Flags]
public enum EntityFlags : uint
{
    None        = 0,
    Alive       = 1 << 0,
    Moving      = 1 << 1,
    Attacking   = 1 << 2,
    Charging    = 1 << 3,
    Stunned     = 1 << 4,
    Invincible  = 1 << 5,
    Selected    = 1 << 6,   // UI용
    Highlighted = 1 << 7,   // UI용
}
```

### 5.3 MotionState (애니메이션 상태)

```csharp
public enum MotionState : byte
{
    Idle,
    Walk,
    Run,
    Attack,
    Skill,
    Hit,
    Death,
    Spawn,
    Stun
}
```

---

## 6. 이벤트 시스템

### 6.1 이벤트 인터페이스

```csharp
namespace UnitSimulator.Contracts.Events;

public interface IGameEvent
{
    int Frame { get; }
    float Time { get; }
    EventType Type { get; }
}

public enum EventType : byte
{
    Spawn,
    Death,
    Attack,
    Hit,
    ProjectileSpawn,
    ProjectileHit,
    SkillActivated,
    AreaEffect
}
```

### 6.2 이벤트 타입별 정의

#### 6.2.1 엔티티 스폰

```csharp
public struct EntitySpawnEvent : IGameEvent
{
    public int Frame { get; init; }
    public float Time { get; init; }
    public EventType Type => EventType.Spawn;

    public int InstanceId;
    public int RefIdHash;
    public EntityType EntityType;
    public Faction Faction;

    public float PosX, PosY, PosZ;

    public int SpawnEffectHash;  // EffectRefId.Hash
    public int SpawnSoundHash;   // SoundRefId.Hash
}
```

#### 6.2.2 엔티티 사망

```csharp
public struct EntityDeathEvent : IGameEvent
{
    public int Frame { get; init; }
    public float Time { get; init; }
    public EventType Type => EventType.Death;

    public int InstanceId;
    public int RefIdHash;
    public int KillerInstanceId;  // -1 if no killer
    public DeathType DeathType;

    public int DeathEffectHash;
    public int DeathSoundHash;
}

public enum DeathType : byte
{
    Normal,
    Explode,
    Dissolve,
    Fade
}
```

#### 6.2.3 공격/피격

```csharp
public struct AttackEvent : IGameEvent
{
    public int Frame { get; init; }
    public float Time { get; init; }
    public EventType Type => EventType.Attack;

    public int AttackerInstanceId;
    public int TargetInstanceId;
    public AttackType AttackType;
    public int Damage;
    public bool IsCritical;

    public int AttackAnimHash;
    public int AttackSoundHash;
}

public struct HitEvent : IGameEvent
{
    public int Frame { get; init; }
    public float Time { get; init; }
    public EventType Type => EventType.Hit;

    public int TargetInstanceId;
    public int AttackerInstanceId;
    public int Damage;
    public DamageType DamageType;
    public bool IsShieldHit;

    public float HitPosX, HitPosY, HitPosZ;
    public float HitDirX, HitDirY, HitDirZ;

    public int HitEffectHash;
    public int HitSoundHash;
}
```

#### 6.2.4 스킬/이펙트

```csharp
public struct SkillActivatedEvent : IGameEvent
{
    public int Frame { get; init; }
    public float Time { get; init; }
    public EventType Type => EventType.SkillActivated;

    public int CasterInstanceId;
    public int SkillRefIdHash;

    public float TargetPosX, TargetPosY, TargetPosZ;
    public int TargetInstanceId;  // -1 if position-based

    public int CastEffectHash;
    public int CastSoundHash;
    public int CastAnimHash;
}

public struct AreaEffectEvent : IGameEvent
{
    public int Frame { get; init; }
    public float Time { get; init; }
    public EventType Type => EventType.AreaEffect;

    public int EffectRefIdHash;
    public float PosX, PosY, PosZ;
    public float Radius;
    public float Duration;

    public int LoopSoundHash;
}
```

---

## 7. 프레임 동기화 전략

### 7.1 접근법 비교

| 접근법 | 장점 | 단점 | 적합한 경우 |
|--------|------|------|-------------|
| **Reference 기반** | 복사 없음, 메모리 효율 | 강한 결합, 스레드 위험 | 단순한 프로젝트 |
| **Value 기반** | 디커플링, 스레드 안전 | 복사 오버헤드, GC | 일반적인 경우 |
| **Pooled Value** | 디커플링 + Zero GC | 구현 복잡도 | **권장** |
| **Delta 기반** | 최소 데이터 전송 | 복잡도 높음 | 대규모 엔티티 |

### 7.2 권장: Pooled Value 기반

```csharp
namespace UnitSimulator.Contracts.Views;

public class FrameSnapshotPool
{
    private readonly EntityViewData[] _entityBuffer;
    private readonly byte[] _eventBuffer;  // 이벤트 직렬화 버퍼

    private int _entityCount;
    private int _eventWritePos;

    public FrameSnapshotPool(int maxEntities = 1024, int eventBufferSize = 8192)
    {
        _entityBuffer = new EntityViewData[maxEntities];
        _eventBuffer = new byte[eventBufferSize];
    }

    public void BeginFrame()
    {
        _entityCount = 0;
        _eventWritePos = 0;
    }

    /// <summary>
    /// 엔티티 뷰 할당 (복사 없이 직접 쓰기)
    /// </summary>
    public ref EntityViewData AllocateEntity()
    {
        return ref _entityBuffer[_entityCount++];
    }

    /// <summary>
    /// 읽기 전용 Span 반환 (복사 없음)
    /// </summary>
    public ReadOnlySpan<EntityViewData> GetEntities()
        => _entityBuffer.AsSpan(0, _entityCount);

    public int EntityCount => _entityCount;
}
```

### 7.3 이벤트 Ring Buffer

```csharp
public class EventRingBuffer<T> where T : unmanaged, IGameEvent
{
    private readonly T[] _buffer;
    private int _writeIndex;
    private int _readIndex;

    public EventRingBuffer(int capacity = 256)
    {
        _buffer = new T[capacity];
    }

    public void Push(in T evt)
    {
        _buffer[_writeIndex % _buffer.Length] = evt;
        _writeIndex++;
    }

    public bool TryPop(out T evt)
    {
        if (_readIndex >= _writeIndex)
        {
            evt = default;
            return false;
        }
        evt = _buffer[_readIndex % _buffer.Length];
        _readIndex++;
        return true;
    }

    public void Clear()
    {
        _readIndex = _writeIndex;
    }
}
```

### 7.4 메모리 사용량 계산

| 항목 | 크기 | 500 엔티티 기준 |
|------|------|-----------------|
| EntityViewData | 64 bytes | 32 KB |
| 이벤트 버퍼 | 8 KB (고정) | 8 KB |
| **총합** | - | **~40 KB/frame** |

---

## 8. Reference 스키마 확장

### 8.1 Visual 정보 추가

기존 `data/references/units.json`에 visual 섹션 추가:

```json
{
  "golem": {
    "displayName": "Golem",
    "maxHP": 5984,
    "damage": 270,
    "moveSpeed": 2.0,
    "attackRange": 60,
    "radius": 40,
    "role": "Melee",
    "layer": "Ground",
    "canTarget": "Ground",
    "skills": ["golem_death_spawn", "golem_death_damage"],

    "visual": {
      "prefabId": "unit_golem",
      "scale": 1.2,
      "animations": {
        "idle": "golem_idle",
        "walk": "golem_walk",
        "attack": "golem_attack_smash",
        "death": "golem_death_crumble"
      },
      "effects": {
        "spawn": "fx_spawn_dust_large",
        "death": "fx_death_rock_explode",
        "hit": "fx_hit_stone"
      },
      "sounds": {
        "attack": "sfx_golem_smash",
        "death": "sfx_golem_crumble",
        "footstep": "sfx_footstep_heavy"
      }
    }
  }
}
```

### 8.2 신규 Reference 파일

#### effects.json

```json
{
  "fx_spawn_dust_large": {
    "type": "particle",
    "duration": 1.5,
    "attachToEntity": false
  },
  "fx_death_rock_explode": {
    "type": "particle",
    "duration": 2.0,
    "shakeCamera": true,
    "shakeIntensity": 0.3
  }
}
```

#### projectiles.json

```json
{
  "arrow_normal": {
    "speed": 800,
    "isHoming": false,
    "trailEffect": "fx_trail_arrow",
    "hitEffect": "fx_hit_arrow",
    "model": "proj_arrow"
  },
  "fireball": {
    "speed": 500,
    "isHoming": true,
    "trailEffect": "fx_trail_fire",
    "hitEffect": "fx_explosion_fire_small",
    "model": "proj_fireball"
  }
}
```

---

## 9. Unity 측 구현 가이드

### 9.1 SimulationBridge 기본 구조

```csharp
public class SimulationBridge : MonoBehaviour
{
    private ISimulation _simulation;
    private Dictionary<int, EntityVisual> _visuals = new();

    [SerializeField] private AssetMappingConfig _assetConfig;

    void FixedUpdate()
    {
        var output = _simulation.Step();

        // 1. 엔티티 동기화
        foreach (ref readonly var entity in output.GetEntities())
        {
            SyncEntity(in entity);
        }

        // 2. 이벤트 처리
        ProcessEvents(output);
    }

    private void SyncEntity(in EntityViewData view)
    {
        if (!_visuals.TryGetValue(view.InstanceId, out var visual))
            return;

        // Transform
        visual.transform.position = new Vector3(view.PosX, view.PosZ, view.PosY);
        visual.transform.rotation = Quaternion.Euler(0, view.RotationY, 0);

        // Animation
        visual.SetMotionState(view.Motion, view.AnimProgress);

        // UI
        visual.HealthBar.SetRatio(view.HealthRatio);
        visual.ShieldBar.SetRatio(view.ShieldRatio);
    }

    private void ProcessEvents(IFrameOutput output)
    {
        // Spawn events
        while (output.TryPopEvent<EntitySpawnEvent>(out var evt))
        {
            OnEntitySpawn(in evt);
        }

        // Death events
        while (output.TryPopEvent<EntityDeathEvent>(out var evt))
        {
            OnEntityDeath(in evt);
        }

        // Hit events
        while (output.TryPopEvent<HitEvent>(out var evt))
        {
            OnHit(in evt);
        }
    }
}
```

### 9.2 Asset Mapping (ScriptableObject)

```csharp
[CreateAssetMenu(fileName = "AssetMapping", menuName = "Simulator/Asset Mapping")]
public class AssetMappingConfig : ScriptableObject
{
    [System.Serializable]
    public class EntityMapping
    {
        public string refId;
        public GameObject prefabBlue;
        public GameObject prefabRed;
        public float modelScale = 1f;
    }

    [System.Serializable]
    public class EffectMapping
    {
        public string refId;
        public ParticleSystem prefab;
        public float duration;
    }

    public List<EntityMapping> entities;
    public List<EffectMapping> effects;

    private Dictionary<int, EntityMapping> _entityCache;
    private Dictionary<int, EffectMapping> _effectCache;

    public void BuildCache()
    {
        _entityCache = entities.ToDictionary(
            e => e.refId.GetHashCode(),
            e => e
        );
        _effectCache = effects.ToDictionary(
            e => e.refId.GetHashCode(),
            e => e
        );
    }

    public GameObject GetPrefab(int refIdHash, Faction faction)
    {
        if (_entityCache.TryGetValue(refIdHash, out var mapping))
        {
            return faction == Faction.Friendly
                ? mapping.prefabBlue
                : mapping.prefabRed;
        }
        return null;
    }
}
```

---

## 10. 구현 우선순위

### Phase 3.2 마일스톤 세분화

| 우선순위 | 작업 | 설명 |
|----------|------|------|
| **P0** | `UnitSimulator.Contracts` 프로젝트 생성 | 공유 모듈 기반 |
| **P0** | RefId 시스템 구현 | 타입 안전한 ID |
| **P0** | EntityViewData 구조체 정의 | 프레임 동기화 핵심 |
| **P1** | FrameSnapshotPool 구현 | Zero GC 프레임 출력 |
| **P1** | 기본 이벤트 타입 구현 | Spawn, Death, Hit |
| **P1** | Core에 MotionState 추가 | 애니메이션 연동 |
| **P2** | Visual Reference 스키마 확장 | effects, sounds 매핑 |
| **P2** | 스킬/투사체 이벤트 구현 | 고급 이벤트 |
| **P3** | Unity 샘플 프로젝트 | 통합 검증 |

---

## 변경 이력

| 날짜 | 버전 | 변경 내용 |
|------|------|-----------|
| 2026-01-20 | 0.1 | 초안 작성 (세션 논의 내용 정리) |

---

## 참조

- [development-milestone.md](../development-milestone.md) - Phase 3 계획
- [CLAUDE.md](../../CLAUDE.md) - 에이전트 행동 규칙
- [data/schemas/](../../data/schemas/) - JSON Schema 정의
