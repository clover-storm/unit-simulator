using System.Collections.Generic;

namespace UnitSimulator;

/// <summary>
/// 유닛 정의를 관리하는 레지스트리.
/// SpawnUnitId로 유닛 정의를 조회하여 실제 유닛 생성에 사용합니다.
/// </summary>
public class UnitRegistry
{
    private readonly Dictionary<string, UnitDefinition> _definitions = new();

    /// <summary>
    /// 등록된 모든 유닛 정의 ID 목록
    /// </summary>
    public IEnumerable<string> RegisteredIds => _definitions.Keys;

    /// <summary>
    /// 유닛 정의를 등록합니다.
    /// </summary>
    public void Register(UnitDefinition definition)
    {
        _definitions[definition.UnitId] = definition;
    }

    /// <summary>
    /// 여러 유닛 정의를 한 번에 등록합니다.
    /// </summary>
    public void RegisterAll(IEnumerable<UnitDefinition> definitions)
    {
        foreach (var def in definitions)
        {
            Register(def);
        }
    }

    /// <summary>
    /// 유닛 정의를 ID로 조회합니다.
    /// </summary>
    /// <returns>정의가 없으면 null 반환</returns>
    public UnitDefinition? GetDefinition(string unitId)
    {
        return _definitions.TryGetValue(unitId, out var def) ? def : null;
    }

    /// <summary>
    /// 유닛 정의가 존재하는지 확인합니다.
    /// </summary>
    public bool HasDefinition(string unitId)
    {
        return _definitions.ContainsKey(unitId);
    }

    /// <summary>
    /// 기본 유닛 정의들이 포함된 레지스트리를 생성합니다.
    /// </summary>
    public static UnitRegistry CreateWithDefaults()
    {
        var registry = new UnitRegistry();
        registry.RegisterAll(GetDefaultDefinitions());
        return registry;
    }

    /// <summary>
    /// 기본 제공되는 유닛 정의 목록.
    /// 클래시로얄 스타일의 기본 유닛들을 정의합니다.
    /// </summary>
    public static IEnumerable<UnitDefinition> GetDefaultDefinitions()
    {
        // Golemite - Golem 사망 시 스폰
        yield return new UnitDefinition
        {
            UnitId = "golemite",
            DisplayName = "Golemite",
            MaxHP = 900,
            Damage = 50,
            AttackRange = 30f,
            MoveSpeed = 3.0f,
            TurnSpeed = 0.1f,
            Radius = 25f,
            Role = UnitRole.Melee,
            Layer = MovementLayer.Ground,
            CanTarget = TargetType.Ground,
            Abilities = new List<AbilityData>
            {
                new DeathDamageData { Damage = 100, Radius = 40f }
            }
        };

        // Skeleton - 묘비, 마녀 등에서 스폰
        yield return new UnitDefinition
        {
            UnitId = "skeleton",
            DisplayName = "Skeleton",
            MaxHP = 81,
            Damage = 81,
            AttackRange = 25f,
            MoveSpeed = 5.0f,
            TurnSpeed = 0.12f,
            Radius = 15f,
            Role = UnitRole.Melee,
            Layer = MovementLayer.Ground,
            CanTarget = TargetType.Ground
        };

        // Lava Pup - Lava Hound 사망 시 스폰
        yield return new UnitDefinition
        {
            UnitId = "lava_pup",
            DisplayName = "Lava Pup",
            MaxHP = 209,
            Damage = 55,
            AttackRange = 60f,
            MoveSpeed = 4.5f,
            TurnSpeed = 0.1f,
            Radius = 15f,
            Role = UnitRole.Ranged,
            Layer = MovementLayer.Air,
            CanTarget = TargetType.GroundAndAir
        };

        // Minion - 일반적인 공중 유닛
        yield return new UnitDefinition
        {
            UnitId = "minion",
            DisplayName = "Minion",
            MaxHP = 252,
            Damage = 84,
            AttackRange = 60f,
            MoveSpeed = 5.0f,
            TurnSpeed = 0.1f,
            Radius = 18f,
            Role = UnitRole.Ranged,
            Layer = MovementLayer.Air,
            CanTarget = TargetType.GroundAndAir
        };

        // Bat - Night Witch 사망 시 스폰
        yield return new UnitDefinition
        {
            UnitId = "bat",
            DisplayName = "Bat",
            MaxHP = 81,
            Damage = 81,
            AttackRange = 25f,
            MoveSpeed = 5.5f,
            TurnSpeed = 0.15f,
            Radius = 12f,
            Role = UnitRole.Melee,
            Layer = MovementLayer.Air,
            CanTarget = TargetType.GroundAndAir
        };

        // Elixir Golemite - Elixir Golem 사망 시 스폰
        yield return new UnitDefinition
        {
            UnitId = "elixir_golemite",
            DisplayName = "Elixir Golemite",
            MaxHP = 560,
            Damage = 42,
            AttackRange = 30f,
            MoveSpeed = 3.5f,
            TurnSpeed = 0.1f,
            Radius = 22f,
            Role = UnitRole.Melee,
            Layer = MovementLayer.Ground,
            CanTarget = TargetType.Ground,
            Abilities = new List<AbilityData>
            {
                new DeathSpawnData
                {
                    SpawnUnitId = "elixir_blob",
                    SpawnCount = 2,
                    SpawnRadius = 20f
                }
            }
        };

        // Elixir Blob - Elixir Golemite 사망 시 스폰
        yield return new UnitDefinition
        {
            UnitId = "elixir_blob",
            DisplayName = "Elixir Blob",
            MaxHP = 280,
            Damage = 21,
            AttackRange = 25f,
            MoveSpeed = 3.5f,
            TurnSpeed = 0.1f,
            Radius = 18f,
            Role = UnitRole.Melee,
            Layer = MovementLayer.Ground,
            CanTarget = TargetType.Ground
        };

        // Guard - 쉴드를 가진 스켈레톤
        yield return new UnitDefinition
        {
            UnitId = "guard",
            DisplayName = "Guard",
            MaxHP = 90,
            Damage = 90,
            AttackRange = 30f,
            MoveSpeed = 4.5f,
            TurnSpeed = 0.1f,
            Radius = 18f,
            Role = UnitRole.Melee,
            Layer = MovementLayer.Ground,
            CanTarget = TargetType.Ground,
            Abilities = new List<AbilityData>
            {
                new ShieldData { MaxShieldHP = 199 }
            }
        };
    }
}
