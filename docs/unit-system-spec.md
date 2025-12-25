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

## 11. References

- [Clash Royale Wiki - Cards](https://clashroyale.fandom.com/wiki/Cards)
- [Clash Royale Wiki - Troop Cards](https://clashroyale.fandom.com/wiki/Category:Troop_Cards)
- [Clash Royale - Unit Statistics](https://unitstatistics.com/clash-royale/)
