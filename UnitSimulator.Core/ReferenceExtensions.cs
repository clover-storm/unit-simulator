using System.Collections.Generic;
using System.Numerics;

namespace UnitSimulator;

/// <summary>
/// ReferenceModels의 레퍼런스 타입에 대한 확장 메서드
/// </summary>
public static class ReferenceExtensions
{
    /// <summary>
    /// UnitReference를 기반으로 Unit 인스턴스를 생성합니다.
    /// </summary>
    public static Unit CreateUnit(
        this UnitReference unitRef,
        string unitId,
        int id,
        UnitFaction faction,
        Vector2 position,
        ReferenceManager? referenceManager = null)
    {
        var abilities = ConvertSkills(unitRef, referenceManager);
        var unit = new Unit(
            position: position,
            radius: unitRef.Radius,
            speed: unitRef.MoveSpeed,
            turnSpeed: unitRef.TurnSpeed,
            role: unitRef.Role,
            hp: unitRef.MaxHP,
            id: id,
            faction: faction,
            layer: unitRef.Layer,
            canTarget: unitRef.CanTarget,
            damage: unitRef.Damage,
            abilities: abilities,
            unitId: unitId,
            targetPriority: unitRef.TargetPriority
        );
        return unit;
    }

    /// <summary>
    /// SkillReference를 AbilityData로 변환합니다.
    /// </summary>
    public static AbilityData? ToAbilityData(this SkillReference skillRef)
    {
        return skillRef.Type.ToLowerInvariant() switch
        {
            "chargeattack" => new ChargeAttackData
            {
                TriggerDistance = skillRef.TriggerDistance,
                RequiredChargeDistance = skillRef.RequiredChargeDistance,
                DamageMultiplier = skillRef.DamageMultiplier,
                SpeedMultiplier = skillRef.SpeedMultiplier
            },
            "splashdamage" => new SplashDamageData
            {
                Radius = skillRef.Radius,
                DamageFalloff = skillRef.DamageFalloff
            },
            "shield" => new ShieldData
            {
                MaxShieldHP = skillRef.MaxShieldHP,
                BlocksStun = skillRef.BlocksStun,
                BlocksKnockback = skillRef.BlocksKnockback
            },
            "deathspawn" => new DeathSpawnData
            {
                SpawnUnitId = skillRef.SpawnUnitId,
                SpawnCount = skillRef.SpawnCount,
                SpawnRadius = skillRef.SpawnRadius,
                SpawnUnitHP = skillRef.SpawnUnitHP
            },
            "deathdamage" => new DeathDamageData
            {
                Damage = skillRef.Damage,
                Radius = skillRef.Radius,
                KnockbackDistance = skillRef.KnockbackDistance
            },
            _ => null
        };
    }

    private static List<AbilityData> ConvertSkills(UnitReference unitRef, ReferenceManager? referenceManager)
    {
        var result = new List<AbilityData>();
        if (referenceManager?.Skills == null || unitRef.Skills.Count == 0)
        {
            return result;
        }

        foreach (var skillId in unitRef.Skills)
        {
            if (!referenceManager.Skills.TryGet(skillId, out var skillRef) || skillRef == null)
            {
                continue;
            }

            var ability = skillRef.ToAbilityData();
            if (ability != null)
            {
                result.Add(ability);
            }
        }
        return result;
    }
}
