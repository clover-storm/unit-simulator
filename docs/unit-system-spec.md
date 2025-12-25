# Unit System Extension Spec

클래시로얄 스타일의 게임 유닛을 표현하기 위한 Core 모듈 확장 스펙입니다.

## 1. 현재 시스템 분석

### 1.1 현재 구현 상태

| 구분 | 현재 구현 |
|------|----------|
| 유닛 클래스 | 단일 `Unit` 클래스 (상속 없음) |
| UnitRole | `Melee`, `Ranged` (2가지) |
| UnitFaction | `Friendly`, `Enemy` |
| 속성 | Position, Velocity, HP, Speed, AttackRange, Damage, TurnSpeed |
| 이동 유형 | Ground만 (2D 평면) |
| 특수 능력 | 없음 |

### 1.2 클래시로얄 유닛 시스템

| 구분 | 클래시로얄 |
|------|-----------|
| 카드 유형 | Troops, Buildings, Spells, Tower Troops, Champions |
| 유닛 수 | 87 Troops, 13 Buildings, 21 Spells (총 125장) |
| 이동 유형 | Ground / Air |
| 공격 대상 | GroundOnly, AirOnly, Both, BuildingsOnly |
| 특수 메커니즘 | Charge, Shield, Stun, Knockback, Slow, DeathSpawn, Spawner |

---

## 2. Entity Type System

### 2.1 EntityType (엔티티 유형)

```csharp
public enum EntityType
{
    Troop,      // 이동 가능한 유닛
    Building,   // 고정 구조물 (수명 있음)
    Spell,      // 즉시 효과 (일회성)
    Projectile  // 투사체
}
```

### 2.2 MovementLayer (이동 레이어)

```csharp
public enum MovementLayer
{
    Ground,     // 지상 유닛 - 지형/충돌 영향 받음
    Air         // 공중 유닛 - 지형 무시, 공중 충돌만 적용
}
```

**적용 규칙:**
- Ground 유닛은 장애물, 강(River) 등을 통과할 수 없음
- Air 유닛은 모든 지형을 통과하며, Air 유닛끼리만 충돌
- Air 유닛도 목적지 도달 시 정지 (hovering)

### 2.3 TargetType (공격 대상 유형)

```csharp
[Flags]
public enum TargetType
{
    None      = 0,
    Ground    = 1 << 0,   // 지상 유닛 공격 가능
    Air       = 1 << 1,   // 공중 유닛 공격 가능
    Building  = 1 << 2,   // 건물 공격 가능

    GroundAndAir = Ground | Air,
    All = Ground | Air | Building
}
```

**유닛별 TargetType 예시:**
| 유닛 | CanTarget |
|------|-----------|
| Knight | Ground |
| Musketeer | GroundAndAir |
| Inferno Dragon | GroundAndAir |
| Hog Rider | Building |
| Baby Dragon | GroundAndAir |

---

## 3. Unit Role System

### 3.1 UnitRole 확장

```csharp
public enum UnitRole
{
    // === 기존 (공격 방식) ===
    Melee,          // 근접 공격
    Ranged,         // 원거리 공격

    // === 신규 (전술적 역할) ===
    Tank,           // 높은 HP, 낮은 DPS - 피해 흡수 역할
    MiniTank,       // 중간 HP, 중간 DPS - 범용 전투
    GlassCannon,    // 낮은 HP, 높은 DPS - 보호 필요
    Swarm,          // 다수 유닛, 낮은 개별 HP - 수량으로 압도
    Spawner,        // 유닛 생성 능력 보유
    Support,        // 버프/디버프/힐 제공
    Siege           // 건물 전용 공격 (타워 러시)
}
```

### 3.2 AttackType (공격 방식)

UnitRole에서 공격 방식을 분리하여 별도 enum으로 관리:

```csharp
public enum AttackType
{
    Melee,          // 근접 공격 (range = radius * 3)
    MeleeShort,     // 초근접 (range = radius * 2)
    MeleeMedium,    // 중거리 근접 (range = radius * 4)
    MeleeLong,      // 장거리 근접 (range = radius * 5)
    Ranged,         // 원거리 공격 (range = radius * 6+)
    None            // 공격 불가 (Elixir Collector 등)
}
```

---

## 4. Special Abilities System

### 4.1 AbilityType

```csharp
public enum AbilityType
{
    // === 공격 관련 ===
    ChargeAttack,       // 돌진 공격 - 일정 거리 이동 후 2배 데미지
    SplashDamage,       // 범위 공격 - 주변 적에게 피해
    ChainDamage,        // 연쇄 공격 - 근처 적에게 순차 피해
    PiercingAttack,     // 관통 공격 - 여러 적 동시 피해

    // === 상태 효과 부여 ===
    StunOnHit,          // 공격 시 기절 부여
    SlowOnHit,          // 공격 시 감속 부여
    KnockbackOnHit,     // 공격 시 넉백 부여
    PoisonOnHit,        // 공격 시 독 부여

    // === 방어 관련 ===
    Shield,             // 분리된 쉴드 HP 보유
    DamageReduction,    // 피해 감소 (조건부)
    DeathDamageImmune,  // 죽음 피해 면역

    // === 죽음 효과 ===
    DeathSpawn,         // 죽을 때 유닛 생성
    DeathDamage,        // 죽을 때 폭발 피해
    DeathEffect,        // 죽을 때 효과 발동 (슬로우 등)

    // === 생성 관련 ===
    SpawnUnits,         // 주기적 유닛 생성
    SpawnOnDeploy,      // 배치 시 추가 유닛 생성

    // === 이동 관련 ===
    Dash,               // 대시 (순간 이동 + 무적)
    Jump,               // 점프 공격 (범위 피해)
    Tunnel,             // 지하 이동 (맵 어디든)
    Charge,             // 돌진 (이동 속도 증가)

    // === 특수 ===
    Invisibility,       // 은신 (타겟팅 불가)
    Rage,               // 격노 (주변 아군 버프)
    Heal,               // 치유 (주변 아군 회복)
    Reflect             // 반사 (피해 반사)
}
```

### 4.2 Ability 데이터 구조

```csharp
public class AbilityData
{
    public AbilityType Type { get; init; }
    public float Magnitude { get; init; }      // 효과 크기 (데미지 배율, 감속률 등)
    public float Duration { get; init; }       // 지속 시간 (초)
    public float Cooldown { get; init; }       // 쿨다운 (초)
    public float Range { get; init; }          // 효과 범위
    public float TriggerDistance { get; init; } // 발동 거리 (Charge용)
    public string SpawnUnitId { get; init; }   // 생성할 유닛 ID (Spawn용)
    public int SpawnCount { get; init; }       // 생성 수량
}
```

### 4.3 주요 능력 상세

#### ChargeAttack (돌진 공격)
- **발동 조건**: 타겟과의 거리가 `TriggerDistance` 이상일 때
- **효과**: 이동 속도 2배 증가, 도달 시 `Magnitude`배 데미지
- **예시**: Prince (2배 데미지), Dark Prince (2배 데미지 + Shield)

```csharp
public class ChargeState
{
    public bool IsCharging { get; set; }
    public float ChargeDistance { get; set; }  // 현재까지 돌진 거리
    public float RequiredDistance { get; set; } // 필요 돌진 거리
}
```

#### Shield (쉴드)
- **효과**: 메인 HP 이전에 소모되는 별도 HP
- **특성**: 쉴드가 남아있는 동안 스턴/넉백 면역 가능
- **예시**: Dark Prince (240 Shield), Guards (199 Shield)

```csharp
public class ShieldData
{
    public int MaxShieldHP { get; init; }
    public int CurrentShieldHP { get; set; }
    public bool BlocksStun { get; init; }      // 스턴 방어 여부
    public bool BlocksKnockback { get; init; } // 넉백 방어 여부
}
```

#### DeathSpawn (죽음 생성)
- **발동 조건**: 유닛 사망 시
- **효과**: 지정된 유닛을 지정 수량만큼 생성
- **예시**: Golem → 2 Golemites, Lava Hound → 6 Lava Pups

```csharp
public class DeathSpawnData
{
    public string SpawnUnitId { get; init; }
    public int SpawnCount { get; init; }
    public float SpawnRadius { get; init; }    // 생성 범위
}
```

#### SplashDamage (범위 공격)
- **효과**: 타겟 주변 반경 내 모든 적에게 피해
- **예시**: Wizard (1.5 tile radius), Baby Dragon (1 tile radius)

```csharp
public class SplashData
{
    public float Radius { get; init; }         // 스플래시 반경
    public float DamageFalloff { get; init; }  // 거리별 피해 감소율 (0-1)
}
```

---

## 5. Status Effect System

### 5.1 StatusEffectType

```csharp
public enum StatusEffectType
{
    // === 이동/행동 제한 ===
    Stunned,        // 기절 - 모든 행동 불가
    Frozen,         // 빙결 - 모든 행동 불가 + 시각 효과
    Slowed,         // 감속 - 이동/공격 속도 감소
    Rooted,         // 속박 - 이동 불가, 공격 가능

    // === 피해 관련 ===
    Poisoned,       // 중독 - 지속 피해
    Burning,        // 화상 - 지속 피해 (건물에 추가 피해)

    // === 버프 ===
    Raged,          // 격노 - 이동/공격 속도 증가
    Healing,        // 치유 - 지속 회복
    Shielded,       // 보호막 - 일시적 쉴드
    Invisible,      // 은신 - 타겟팅 불가

    // === 특수 ===
    Marked,         // 표식 - 추가 피해 받음
    Invulnerable    // 무적 - 피해 면역
}
```

### 5.2 StatusEffect 데이터 구조

```csharp
public class StatusEffect
{
    public StatusEffectType Type { get; init; }
    public float Duration { get; set; }        // 남은 지속 시간
    public float Magnitude { get; init; }      // 효과 크기 (감속률, 피해량 등)
    public float TickInterval { get; init; }   // DOT 틱 간격
    public Unit Source { get; init; }          // 효과 부여자
    public bool IsStackable { get; init; }     // 중첩 가능 여부
}
```

### 5.3 상태 효과 적용 규칙

| 효과 | Magnitude 의미 | 중첩 | 비고 |
|------|---------------|------|------|
| Stunned | - | X | 지속시간 갱신 |
| Frozen | - | X | 지속시간 갱신 |
| Slowed | 감속률 (0.35 = 35%) | X | 가장 강한 효과 적용 |
| Poisoned | 초당 피해량 | O | 최대 3스택 |
| Raged | 속도 증가율 (0.35 = 35%) | X | |
| Invisible | - | X | 공격 시 해제 |

---

## 6. Building System

### 6.1 BuildingType

```csharp
public enum BuildingType
{
    Defensive,      // 공격 가능한 방어 건물
    Spawner,        // 유닛 생성 건물
    Utility         // 유틸리티 건물 (Elixir Collector)
}
```

### 6.2 Building 데이터 구조

```csharp
public class BuildingStats
{
    public BuildingType Type { get; init; }
    public float Lifetime { get; init; }           // 수명 (초)
    public float CurrentLifetime { get; set; }     // 남은 수명

    // Spawner 전용
    public string SpawnUnitId { get; init; }       // 생성 유닛 ID
    public int SpawnCount { get; init; }           // 회당 생성 수
    public float SpawnInterval { get; init; }      // 생성 주기 (초)
    public float FirstSpawnDelay { get; init; }    // 첫 생성 딜레이

    // Defensive 전용
    public float AttackRange { get; init; }
    public int Damage { get; init; }
    public float AttackSpeed { get; init; }
    public TargetType CanTarget { get; init; }
}
```

### 6.3 건물 유형별 예시

| 건물 | Type | Lifetime | 특성 |
|------|------|----------|------|
| Cannon | Defensive | 30s | Ground만 공격 |
| Tesla | Defensive | 35s | Ground+Air, 숨김 상태 |
| Inferno Tower | Defensive | 40s | 시간 비례 피해 증가 |
| Goblin Hut | Spawner | 40s | 5초마다 Spear Goblin 생성 |
| Tombstone | Spawner | 40s | 3초마다 Skeleton 생성, 파괴 시 4 Skeleton |
| Elixir Collector | Utility | 70s | 8.5초마다 1 Elixir 생성 |

---

## 7. Spell System

### 7.1 SpellType

```csharp
public enum SpellType
{
    Instant,        // 즉시 피해 (Fireball, Zap)
    AreaOverTime,   // 지속 효과 (Poison, Earthquake)
    Utility,        // 유틸리티 (Freeze, Rage, Tornado)
    Spawning        // 유닛 소환 (Goblin Barrel, Graveyard)
}
```

### 7.2 Spell 데이터 구조

```csharp
public class SpellStats
{
    public SpellType Type { get; init; }
    public float Radius { get; init; }             // 효과 범위
    public float Duration { get; init; }           // 지속 시간
    public float CastDelay { get; init; }          // 시전 딜레이

    // 피해 관련
    public int Damage { get; init; }               // 즉시 피해
    public int DamagePerTick { get; init; }        // 틱당 피해
    public float TickInterval { get; init; }       // 틱 간격
    public float BuildingDamageMultiplier { get; init; } // 건물 피해 배율

    // 효과 관련
    public TargetType AffectedTargets { get; init; }
    public StatusEffectType AppliedEffect { get; init; }
    public float EffectMagnitude { get; init; }

    // 소환 관련
    public string SpawnUnitId { get; init; }
    public int SpawnCount { get; init; }
    public float SpawnInterval { get; init; }
}
```

### 7.3 스펠 예시

| 스펠 | Type | Radius | 효과 |
|------|------|--------|------|
| Fireball | Instant | 2.5 | 572 피해 + 넉백 |
| Zap | Instant | 2.5 | 159 피해 + 0.5s 스턴 |
| Poison | AreaOverTime | 3.5 | 8초간 초당 95 피해 + 감속 |
| Freeze | Utility | 3 | 4초간 빙결 |
| Tornado | Utility | 5.5 | 2초간 중앙으로 끌어당김 |
| Graveyard | Spawning | 4 | 10초간 Skeleton 생성 |

---

## 8. Extended UnitStats

### 8.1 완전한 UnitStats 구조

```csharp
public class UnitStats
{
    // === 기본 정보 ===
    public string UnitId { get; init; }            // 고유 식별자
    public string DisplayName { get; init; }       // 표시명
    public EntityType EntityType { get; init; }    // 엔티티 유형
    public UnitRole Role { get; init; }            // 전술적 역할
    public AttackType AttackType { get; init; }    // 공격 방식

    // === 이동 관련 ===
    public MovementLayer Layer { get; init; }      // 이동 레이어
    public float MoveSpeed { get; init; }          // 이동 속도
    public float TurnSpeed { get; init; }          // 회전 속도

    // === 생존 관련 ===
    public int MaxHP { get; init; }                // 최대 HP
    public int ShieldHP { get; init; }             // 쉴드 HP (0이면 없음)
    public float Radius { get; init; }             // 충돌 반경

    // === 전투 관련 ===
    public int Damage { get; init; }               // 기본 공격력
    public float AttackSpeed { get; init; }        // 공격 속도 (초당)
    public float AttackRange { get; init; }        // 공격 사거리
    public TargetType CanTarget { get; init; }     // 공격 가능 대상

    // === 스웜 유닛 ===
    public int SpawnCount { get; init; }           // 배치 시 생성 수 (1이면 단일)

    // === 특수 능력 ===
    public List<AbilityData> Abilities { get; init; }

    // === 건물 전용 ===
    public BuildingStats BuildingStats { get; init; }
}
```

### 8.2 Unit 클래스 확장

```csharp
public class Unit
{
    // === 기존 속성 ===
    public int Id { get; init; }
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public Vector2 Forward { get; set; }
    public UnitFaction Faction { get; init; }

    // === 스탯 참조 ===
    public UnitStats Stats { get; init; }

    // === 현재 상태 ===
    public int CurrentHP { get; set; }
    public int CurrentShieldHP { get; set; }
    public bool IsDead { get; set; }
    public Unit Target { get; set; }
    public float AttackCooldown { get; set; }

    // === 상태 효과 ===
    public List<StatusEffect> ActiveEffects { get; } = new();

    // === 능력 상태 ===
    public ChargeState ChargeState { get; set; }
    public bool IsInvisible { get; set; }
    public Dictionary<AbilityType, float> AbilityCooldowns { get; } = new();

    // === 건물 상태 (Building인 경우) ===
    public float RemainingLifetime { get; set; }
    public float SpawnTimer { get; set; }
}
```

---

## 9. Implementation Roadmap

### 9.0 Current Implementation Status (Core)

**완료:**
- Phase 1: Ground/Air 레이어, TargetType 필터링, 레이어별 충돌/분리 로직 구현 완료.
- Phase 2 (부분): SplashDamage, Shield HP, ChargeAttack(속도/배율 적용), DeathSpawn/DeathDamage(사망 시 스폰/폭발, 스폰은 현재 기본 근접 스탯으로 생성) 구현 및 직렬화 반영.

**진행 예정:**
- **2-Phase Update 아키텍처 적용** (Section 11 참조): 즉시 적용 방식에서 이벤트 수집 후 일괄 적용 방식으로 전환. 순서 독립성 및 연쇄 사망 처리 개선.

**미구현:**
- Ability 데이터 기반 스폰 유닛 정의 로딩
- Status Effects/Buildings/Spells (Phase 3~5)
- Projectile/Factory 파이프라인

### Phase 1: Foundation (기초)

**목표**: Ground/Air 레이어 및 타겟팅 시스템

| 항목 | 설명 | 난이도 |
|------|------|--------|
| `MovementLayer` enum | Ground/Air 구분 | 낮음 |
| `TargetType` flags | 공격 대상 필터링 | 낮음 |
| Unit.CanTarget 검증 | 타겟 유효성 검사 | 중간 |
| Air 유닛 이동 로직 | 지형 충돌 무시 | 중간 |

**예상 변경 파일**:
- `Unit.cs`: MovementLayer, TargetType 속성 추가
- `SquadBehavior.cs`: 타겟 필터링 로직
- `EnemyBehavior.cs`: 타겟 필터링 로직

### Phase 2: Combat Mechanics (전투 메커니즘)

**목표**: 핵심 전투 능력 구현

| 항목 | 설명 | 난이도 |
|------|------|--------|
| SplashDamage | 범위 공격 시스템 | 중간 |
| Shield | 쉴드 HP 분리 | 중간 |
| ChargeAttack | 돌진 공격 메커니즘 | 높음 |
| DeathSpawn | 죽음 시 유닛 생성 | 중간 |

**예상 변경 파일**:
- `Unit.cs`: ShieldHP, ChargeState 추가
- `CombatSystem.cs` (신규): 전투 로직 분리
- `AbilityProcessor.cs` (신규): 능력 처리기

### Phase 3: Status Effects (상태 효과)

**목표**: 상태 효과 시스템 구현

| 항목 | 설명 | 난이도 |
|------|------|--------|
| StatusEffect 클래스 | 상태 효과 데이터 | 낮음 |
| 효과 적용/해제 로직 | 프레임별 처리 | 중간 |
| Stun/Slow/Freeze | 행동 제한 효과 | 중간 |
| Knockback | 넉백 물리 처리 | 높음 |

**예상 변경 파일**:
- `StatusEffectSystem.cs` (신규): 상태 효과 관리
- `Unit.cs`: ActiveEffects 리스트 추가
- 행동 클래스들: 상태 효과 체크 로직

### Phase 4: Buildings & Spells (건물/스펠)

**목표**: 건물 및 스펠 엔티티 구현

| 항목 | 설명 | 난이도 |
|------|------|--------|
| Building 엔티티 | 고정 구조물, 수명 | 중간 |
| Spawner 로직 | 주기적 유닛 생성 | 중간 |
| Spell 시스템 | 즉시/지속 효과 | 높음 |
| Projectile 시스템 | 투사체 처리 | 높음 |

**예상 신규 파일**:
- `Building.cs`: 건물 엔티티
- `Spell.cs`: 스펠 엔티티
- `Projectile.cs`: 투사체 엔티티
- `SpawnerSystem.cs`: 생성 로직

### Phase 5: Data-Driven (데이터 기반)

**목표**: JSON/XML 기반 유닛 정의

| 항목 | 설명 | 난이도 |
|------|------|--------|
| UnitDefinition JSON | 유닛 스탯 외부화 | 중간 |
| AbilityDefinition | 능력 데이터 외부화 | 중간 |
| UnitFactory | 정의 기반 유닛 생성 | 중간 |
| Balance Loader | 밸런스 데이터 로드 | 낮음 |

---

## 10. Sample Unit Definitions

### 10.1 Knight (기사)

```json
{
  "unitId": "knight",
  "displayName": "Knight",
  "entityType": "Troop",
  "role": "MiniTank",
  "attackType": "Melee",
  "layer": "Ground",
  "canTarget": ["Ground"],
  "stats": {
    "maxHP": 1938,
    "damage": 202,
    "attackSpeed": 1.1,
    "attackRange": 60,
    "moveSpeed": 60,
    "radius": 20
  },
  "abilities": []
}
```

### 10.2 Baby Dragon (베이비 드래곤)

```json
{
  "unitId": "baby_dragon",
  "displayName": "Baby Dragon",
  "entityType": "Troop",
  "role": "Support",
  "attackType": "Ranged",
  "layer": "Air",
  "canTarget": ["Ground", "Air"],
  "stats": {
    "maxHP": 1152,
    "damage": 160,
    "attackSpeed": 1.5,
    "attackRange": 180,
    "moveSpeed": 60,
    "radius": 25
  },
  "abilities": [
    {
      "type": "SplashDamage",
      "radius": 60,
      "damageFalloff": 0
    }
  ]
}
```

### 10.3 Prince (프린스)

```json
{
  "unitId": "prince",
  "displayName": "Prince",
  "entityType": "Troop",
  "role": "GlassCannon",
  "attackType": "MeleeMedium",
  "layer": "Ground",
  "canTarget": ["Ground"],
  "stats": {
    "maxHP": 1669,
    "damage": 392,
    "attackSpeed": 1.4,
    "attackRange": 80,
    "moveSpeed": 60,
    "radius": 25
  },
  "abilities": [
    {
      "type": "ChargeAttack",
      "triggerDistance": 150,
      "magnitude": 2.0,
      "chargeSpeedMultiplier": 2.0
    }
  ]
}
```

### 10.4 Golem (골렘)

```json
{
  "unitId": "golem",
  "displayName": "Golem",
  "entityType": "Troop",
  "role": "Tank",
  "attackType": "Melee",
  "layer": "Ground",
  "canTarget": ["Building"],
  "stats": {
    "maxHP": 5984,
    "damage": 270,
    "attackSpeed": 2.5,
    "attackRange": 60,
    "moveSpeed": 30,
    "radius": 40
  },
  "abilities": [
    {
      "type": "DeathSpawn",
      "spawnUnitId": "golemite",
      "spawnCount": 2
    },
    {
      "type": "DeathDamage",
      "damage": 270,
      "radius": 60
    }
  ]
}
```

### 10.5 Tombstone (묘비)

```json
{
  "unitId": "tombstone",
  "displayName": "Tombstone",
  "entityType": "Building",
  "buildingType": "Spawner",
  "stats": {
    "maxHP": 511,
    "radius": 30
  },
  "buildingStats": {
    "lifetime": 40,
    "spawnUnitId": "skeleton",
    "spawnCount": 1,
    "spawnInterval": 3.0,
    "firstSpawnDelay": 1.0
  },
  "abilities": [
    {
      "type": "DeathSpawn",
      "spawnUnitId": "skeleton",
      "spawnCount": 4
    }
  ]
}
```

---

## 11. Simulation Loop Architecture

### 11.1 설계 원칙

시뮬레이션의 결정론적(deterministic) 동작과 순서 독립성을 보장하기 위해 **2-Phase Update** 패턴을 적용합니다.

**핵심 원칙:**
- 업데이트 도중 유닛 상태(HP, IsDead 등)를 직접 변경하지 않음
- 모든 공격/피해 결과는 이벤트로 수집 후 일괄 적용
- 사망 판정은 모든 유닛 틱이 완료된 후 수행

### 11.2 문제점: 즉시 적용 방식

```
[기존 방식 - 문제점]
Frame N:
  Enemy A 업데이트 → Friendly B 공격 → B.HP 즉시 감소 → B 사망
  Enemy C 업데이트 → B를 타겟으로 선택하려 했으나 이미 사망
  Friendly B 업데이트 → 자신의 턴이 오기 전에 이미 사망 처리됨
```

**문제점:**
1. **순서 의존성**: 먼저 업데이트되는 유닛이 유리함
2. **동시 사망 불가**: A와 B가 서로 죽이는 경우, 먼저 처리된 쪽만 공격 성공
3. **연쇄 효과 복잡**: DeathDamage로 인한 2차 사망 처리가 재귀적으로 발생
4. **상태 불일치**: 같은 프레임 내에서 유닛이 살아있는지 여부가 시점에 따라 다름

### 11.3 해결책: 2-Phase Update 패턴

```
[새로운 방식]
Frame N:
  ┌─ Phase 1: Collect (상태 변경 없음) ─────────────────┐
  │  Enemy A 업데이트 → DamageEvent(A→B, 50) 생성       │
  │  Enemy C 업데이트 → DamageEvent(C→B, 30) 생성       │
  │  Friendly B 업데이트 → DamageEvent(B→A, 40) 생성    │
  │  (모든 유닛이 프레임 시작 시점의 상태를 기준으로 행동)  │
  └─────────────────────────────────────────────────────┘

  ┌─ Phase 2: Apply (일괄 적용) ────────────────────────┐
  │  1. 모든 DamageEvent 적용 → HP 감소                 │
  │  2. HP <= 0 유닛 사망 판정                          │
  │  3. 사망 유닛의 Death 어빌리티 처리 (연쇄 포함)       │
  │  4. SpawnEvent 적용 → 유닛 생성                     │
  └─────────────────────────────────────────────────────┘
```

### 11.4 FrameEvents 구조

```csharp
/// <summary>
/// 프레임 내 발생하는 모든 이벤트를 수집하는 컨테이너
/// </summary>
public class FrameEvents
{
    public List<DamageEvent> Damages { get; } = new();
    public List<SpawnRequest> Spawns { get; } = new();
    public List<HealEvent> Heals { get; } = new();  // 향후 확장
}

public class DamageEvent
{
    public Unit Source { get; init; }      // 피해 원인 유닛
    public Unit Target { get; init; }      // 피해 대상 유닛
    public int Amount { get; init; }       // 피해량
    public DamageType Type { get; init; }  // Normal, Splash, DeathDamage
}

public enum DamageType
{
    Normal,       // 일반 공격
    Splash,       // 스플래시 피해
    DeathDamage,  // 사망 시 폭발 피해
    Spell         // 스펠 피해
}
```

### 11.5 Phase 1: Collect

모든 유닛이 **프레임 시작 시점의 상태**를 기준으로 행동을 결정합니다.

```csharp
public void UpdateFriendlySquad(..., FrameEvents events)
{
    foreach (var friendly in friendlies)
    {
        // 이동 처리 (상태 변경 허용 - Position, Velocity)
        UpdateMovement(friendly);

        // 공격 처리 (상태 변경 금지 - 이벤트만 생성)
        if (CanAttack(friendly, friendly.Target))
        {
            int damage = friendly.GetEffectiveDamage();
            events.Damages.Add(new DamageEvent
            {
                Source = friendly,
                Target = friendly.Target,
                Amount = damage,
                Type = DamageType.Normal
            });

            // 스플래시 피해도 이벤트로 수집
            if (friendly.HasAbility<SplashDamageData>())
            {
                CollectSplashDamage(friendly, damage, events);
            }
        }
    }
}
```

**Phase 1에서 허용되는 상태 변경:**
- Position, Velocity (이동)
- Forward (회전)
- AttackCooldown (쿨다운 감소)
- Target (타겟 변경)

**Phase 1에서 금지되는 상태 변경:**
- HP, ShieldHP
- IsDead
- 유닛 생성/제거

### 11.6 Phase 2: Apply

수집된 이벤트를 순차적으로 적용합니다.

```csharp
private void ApplyPhase(FrameEvents events)
{
    // Step 1: 모든 피해 적용 (HP 감소만, 사망 처리 X)
    foreach (var damage in events.Damages)
    {
        if (damage.Target.IsDead) continue;  // 이미 죽은 유닛 스킵
        damage.Target.TakeDamage(damage.Amount);
    }

    // Step 2: 사망 판정 및 Death 어빌리티 처리
    ProcessDeaths(events);

    // Step 3: 스폰 적용
    foreach (var spawn in events.Spawns)
    {
        InjectSpawnedUnit(spawn);
    }
}
```

### 11.7 연쇄 사망 처리 (Queue 기반)

Death 어빌리티(DeathDamage, DeathSpawn)로 인한 연쇄 사망을 **큐 기반**으로 처리합니다.

```csharp
private void ProcessDeaths(FrameEvents events)
{
    var deathQueue = new Queue<Unit>();
    var processed = new HashSet<Unit>();

    // 초기 사망 유닛 수집 (HP <= 0 && !IsDead)
    foreach (var unit in GetAllUnits())
    {
        if (unit.HP <= 0 && !unit.IsDead)
        {
            deathQueue.Enqueue(unit);
        }
    }

    // 큐가 빌 때까지 처리 (연쇄 사망 포함)
    while (deathQueue.Count > 0)
    {
        var dead = deathQueue.Dequeue();
        if (processed.Contains(dead)) continue;

        dead.IsDead = true;
        dead.Velocity = Vector2.Zero;
        processed.Add(dead);

        // DeathSpawn 처리
        var deathSpawn = dead.GetAbility<DeathSpawnData>();
        if (deathSpawn != null)
        {
            events.Spawns.AddRange(CreateSpawnRequests(dead, deathSpawn));
        }

        // DeathDamage 처리 → 추가 사망 유닛 큐에 추가
        var deathDamage = dead.GetAbility<DeathDamageData>();
        if (deathDamage != null)
        {
            foreach (var target in GetUnitsInRadius(dead.Position, deathDamage.Radius))
            {
                if (target.IsDead || target.Faction == dead.Faction) continue;
                target.TakeDamage(deathDamage.Damage);

                if (target.HP <= 0 && !processed.Contains(target))
                {
                    deathQueue.Enqueue(target);
                }
            }
        }
    }
}
```

**흐름 예시:**
```
초기 사망: [A]
  A 처리 → DeathDamage로 B 사망 → Queue: [B]
  B 처리 → DeathDamage로 C, D 사망 → Queue: [C, D]
  C 처리 → 추가 사망 없음 → Queue: [D]
  D 처리 → DeathSpawn으로 E, F 생성 → Queue: []
완료
```

### 11.8 SimulatorCore.Step() 구조

```csharp
public FrameData Step(ISimulatorCallbacks? callbacks = null)
{
    callbacks ??= new DefaultSimulatorCallbacks();
    var events = new FrameEvents();

    // 커맨드 처리 (스폰 명령 등)
    ProcessCommands(callbacks);

    // ════════════════════════════════════════════════════
    // Phase 1: Collect - 모든 유닛 틱, 이벤트 수집
    // ════════════════════════════════════════════════════
    _enemyBehavior.UpdateEnemySquad(this, _enemySquad, _friendlySquad, events);
    _squadBehavior.UpdateFriendlySquad(this, _friendlySquad, _enemySquad, _mainTarget, events);

    // ════════════════════════════════════════════════════
    // Phase 2: Apply - 이벤트 일괄 적용
    // ════════════════════════════════════════════════════
    ApplyDamageEvents(events);
    ProcessDeaths(events);
    ApplySpawnEvents(events, callbacks);

    // 프레임 데이터 생성 및 반환
    var frameData = GenerateFrameData();
    callbacks.OnFrameGenerated(frameData);
    _currentFrame++;

    return frameData;
}
```

### 11.9 이점

| 항목 | 효과 |
|------|------|
| **순서 독립성** | 유닛 업데이트 순서와 무관하게 동일한 결과 |
| **동시 사망 지원** | A→B, B→A 동시 공격 시 둘 다 피해 적용 |
| **연쇄 처리 단순화** | 큐 기반으로 깊이 제한 없이 안전하게 처리 |
| **디버깅 용이** | FrameEvents 로깅으로 프레임 내 모든 이벤트 추적 가능 |
| **확장성** | HealEvent, BuffEvent 등 새 이벤트 타입 추가 용이 |
| **리플레이** | 이벤트 기반으로 리플레이 시스템 구현 용이 |

---

## 12. References

- [Clash Royale Wiki - Cards](https://clashroyale.fandom.com/wiki/Cards)
- [Clash Royale Wiki - Troop Cards](https://clashroyale.fandom.com/wiki/Category:Troop_Cards)
- [Clash Royale - Unit Statistics](https://unitstatistics.com/clash-royale/)
