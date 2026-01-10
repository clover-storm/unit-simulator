using System.Numerics;
using FluentAssertions;
using UnitSimulator.Skills;
using Xunit;

namespace UnitSimulator.Core.Tests.Skills;

public class TowerSkillSystemTests
{
    private readonly TowerSkillSystem _sut = new();

    private static Tower CreateTestTower(int id = 1, UnitFaction faction = UnitFaction.Friendly)
    {
        return new Tower
        {
            Id = id,
            Type = TowerType.Princess,
            Faction = faction,
            Position = new Vector2(100, 100),
            Radius = 20f,
            AttackRange = 100f,
            MaxHP = 1000,
            CurrentHP = 1000,
            Damage = 50,
            AttackSpeed = 1.0f,
            CanTarget = TargetType.GroundAndAir
        };
    }

    private static Unit CreateTestUnit(int id, Vector2 position, int hp = 100)
    {
        return new Unit(
            position: position,
            radius: 10f,
            speed: 5f,
            turnSpeed: 0.1f,
            role: UnitRole.Melee,
            hp: hp,
            id: id,
            faction: UnitFaction.Enemy,
            layer: MovementLayer.Ground,
            canTarget: TargetType.GroundAndAir,
            damage: 10
        );
    }

    private static TowerSkill CreateTargetedDamageSkill(string id = "test_skill", int damage = 100, int cooldownMs = 5000)
    {
        return new TowerSkill
        {
            Id = id,
            Name = "Test Targeted Skill",
            EffectType = SkillEffectType.TargetedDamage,
            TargetType = SkillTargetType.SingleUnit,
            Damage = damage,
            CooldownMs = cooldownMs
        };
    }

    private static TowerSkill CreateAreaSkill(string id = "aoe_skill", int damage = 50, float range = 100f, int cooldownMs = 5000)
    {
        return new TowerSkill
        {
            Id = id,
            Name = "Test AoE Skill",
            EffectType = SkillEffectType.AreaOfEffect,
            TargetType = SkillTargetType.Position,
            Damage = damage,
            Range = range,
            CooldownMs = cooldownMs
        };
    }

    private static TowerSkill CreateNoTargetSkill(string id = "no_target_skill", int damage = 75, float range = 80f)
    {
        return new TowerSkill
        {
            Id = id,
            Name = "Test No Target Skill",
            EffectType = SkillEffectType.AreaOfEffect,
            TargetType = SkillTargetType.None,
            Damage = damage,
            Range = range,
            CooldownMs = 3000
        };
    }

    #region RegisterSkill Tests

    [Fact]
    public void RegisterSkill_AddsSkillToTower()
    {
        var tower = CreateTestTower();
        var skill = CreateTargetedDamageSkill();

        _sut.RegisterSkill(tower.Id, skill);

        var skills = _sut.GetSkills(tower.Id);
        skills.Should().ContainSingle();
        skills[0].Id.Should().Be(skill.Id);
    }

    [Fact]
    public void RegisterSkills_AddsMultipleSkillsToTower()
    {
        var tower = CreateTestTower();
        var skill1 = CreateTargetedDamageSkill("skill1");
        var skill2 = CreateAreaSkill("skill2");

        _sut.RegisterSkills(tower.Id, new[] { skill1, skill2 });

        var skills = _sut.GetSkills(tower.Id);
        skills.Should().HaveCount(2);
    }

    [Fact]
    public void GetSkills_ReturnsEmptyForUnknownTower()
    {
        var skills = _sut.GetSkills(999);
        skills.Should().BeEmpty();
    }

    [Fact]
    public void GetSkill_ReturnsNullForUnknownSkill()
    {
        var tower = CreateTestTower();
        _sut.RegisterSkill(tower.Id, CreateTargetedDamageSkill());

        var skill = _sut.GetSkill(tower.Id, "nonexistent");
        skill.Should().BeNull();
    }

    #endregion

    #region ActivateSkill Tests

    [Fact]
    public void ActivateSkill_ValidInput_ReturnsSuccess()
    {
        var tower = CreateTestTower();
        var skill = CreateTargetedDamageSkill();
        var enemy = CreateTestUnit(1, new Vector2(120, 100));
        _sut.RegisterSkill(tower.Id, skill);

        var result = _sut.ActivateSkill(
            tower.Id,
            skill.Id,
            tower,
            new List<Unit> { enemy },
            targetUnitId: enemy.Id);

        result.Success.Should().BeTrue();
        result.CooldownMs.Should().Be(skill.CooldownMs);
        result.Effects.Should().ContainSingle();
        result.Effects![0].Type.Should().Be("Damage");
        result.Effects[0].Value.Should().Be(skill.Damage);
    }

    [Fact]
    public void ActivateSkill_TowerNotFound_ReturnsError()
    {
        var skill = CreateTargetedDamageSkill();
        _sut.RegisterSkill(1, skill);

        var result = _sut.ActivateSkill(
            towerId: 1,
            skillId: skill.Id,
            tower: null,
            enemies: new List<Unit>());

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(SkillErrorCodes.TowerNotFound);
    }

    [Fact]
    public void ActivateSkill_TowerDestroyed_ReturnsError()
    {
        var tower = CreateTestTower();
        tower.CurrentHP = 0;
        var skill = CreateTargetedDamageSkill();
        _sut.RegisterSkill(tower.Id, skill);

        var result = _sut.ActivateSkill(
            tower.Id,
            skill.Id,
            tower,
            new List<Unit>());

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(SkillErrorCodes.TowerNotFound);
    }

    [Fact]
    public void ActivateSkill_SkillNotFound_ReturnsError()
    {
        var tower = CreateTestTower();
        _sut.RegisterSkill(tower.Id, CreateTargetedDamageSkill("other_skill"));

        var result = _sut.ActivateSkill(
            tower.Id,
            "nonexistent_skill",
            tower,
            new List<Unit>());

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(SkillErrorCodes.SkillNotFound);
    }

    [Fact]
    public void ActivateSkill_SkillOnCooldown_ReturnsError()
    {
        var tower = CreateTestTower();
        var skill = CreateTargetedDamageSkill();
        var enemy = CreateTestUnit(1, new Vector2(120, 100));
        _sut.RegisterSkill(tower.Id, skill);

        _sut.ActivateSkill(tower.Id, skill.Id, tower, new List<Unit> { enemy }, targetUnitId: enemy.Id);

        var secondResult = _sut.ActivateSkill(
            tower.Id,
            skill.Id,
            tower,
            new List<Unit> { enemy },
            targetUnitId: enemy.Id);

        secondResult.Success.Should().BeFalse();
        secondResult.ErrorCode.Should().Be(SkillErrorCodes.SkillOnCooldown);
    }

    [Fact]
    public void ActivateSkill_TargetedSkillWithoutTarget_ReturnsError()
    {
        var tower = CreateTestTower();
        var skill = CreateTargetedDamageSkill();
        _sut.RegisterSkill(tower.Id, skill);

        var result = _sut.ActivateSkill(
            tower.Id,
            skill.Id,
            tower,
            new List<Unit>(),
            targetUnitId: null);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(SkillErrorCodes.TargetRequired);
    }

    [Fact]
    public void ActivateSkill_PositionSkillWithoutPosition_ReturnsError()
    {
        var tower = CreateTestTower();
        var skill = CreateAreaSkill();
        _sut.RegisterSkill(tower.Id, skill);

        var result = _sut.ActivateSkill(
            tower.Id,
            skill.Id,
            tower,
            new List<Unit>(),
            targetPosition: null);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(SkillErrorCodes.TargetRequired);
    }

    [Fact]
    public void ActivateSkill_TargetUnitNotFound_ReturnsError()
    {
        var tower = CreateTestTower();
        var skill = CreateTargetedDamageSkill();
        _sut.RegisterSkill(tower.Id, skill);

        var result = _sut.ActivateSkill(
            tower.Id,
            skill.Id,
            tower,
            new List<Unit>(),
            targetUnitId: 999);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(SkillErrorCodes.TargetNotFound);
    }

    [Fact]
    public void ActivateSkill_TargetUnitDead_ReturnsError()
    {
        var tower = CreateTestTower();
        var skill = CreateTargetedDamageSkill();
        var deadEnemy = CreateTestUnit(1, new Vector2(120, 100));
        deadEnemy.IsDead = true;
        _sut.RegisterSkill(tower.Id, skill);

        var result = _sut.ActivateSkill(
            tower.Id,
            skill.Id,
            tower,
            new List<Unit> { deadEnemy },
            targetUnitId: deadEnemy.Id);

        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be(SkillErrorCodes.TargetNotFound);
    }

    #endregion

    #region Area Damage Tests

    [Fact]
    public void ActivateSkill_AreaSkill_AffectsUnitsInRange()
    {
        var tower = CreateTestTower();
        var skill = CreateAreaSkill(damage: 50, range: 100f);
        var enemyInRange = CreateTestUnit(1, new Vector2(150, 100), hp: 200);
        var enemyOutOfRange = CreateTestUnit(2, new Vector2(300, 100), hp: 200);
        _sut.RegisterSkill(tower.Id, skill);

        var result = _sut.ActivateSkill(
            tower.Id,
            skill.Id,
            tower,
            new List<Unit> { enemyInRange, enemyOutOfRange },
            targetPosition: new Vector2(150, 100));

        result.Success.Should().BeTrue();
        result.Effects.Should().ContainSingle();
        result.Effects![0].TargetId.Should().Be(enemyInRange.Id.ToString());
        enemyInRange.HP.Should().Be(150);
        enemyOutOfRange.HP.Should().Be(200);
    }

    [Fact]
    public void ActivateSkill_AreaSkillNoTarget_UsesTowerPosition()
    {
        var tower = CreateTestTower();
        var skill = CreateNoTargetSkill(damage: 30, range: 50f);
        var nearEnemy = CreateTestUnit(1, new Vector2(120, 100), hp: 100);
        var farEnemy = CreateTestUnit(2, new Vector2(200, 100), hp: 100);
        _sut.RegisterSkill(tower.Id, skill);

        var result = _sut.ActivateSkill(
            tower.Id,
            skill.Id,
            tower,
            new List<Unit> { nearEnemy, farEnemy });

        result.Success.Should().BeTrue();
        result.Effects.Should().ContainSingle();
        nearEnemy.HP.Should().Be(70);
        farEnemy.HP.Should().Be(100);
    }

    #endregion

    #region Cooldown Tests

    [Fact]
    public void IsSkillOnCooldown_BeforeActivation_ReturnsFalse()
    {
        var tower = CreateTestTower();
        var skill = CreateTargetedDamageSkill();
        _sut.RegisterSkill(tower.Id, skill);

        _sut.IsSkillOnCooldown(tower.Id, skill.Id).Should().BeFalse();
    }

    [Fact]
    public void IsSkillOnCooldown_AfterActivation_ReturnsTrue()
    {
        var tower = CreateTestTower();
        var skill = CreateTargetedDamageSkill();
        var enemy = CreateTestUnit(1, new Vector2(120, 100));
        _sut.RegisterSkill(tower.Id, skill);

        _sut.ActivateSkill(tower.Id, skill.Id, tower, new List<Unit> { enemy }, targetUnitId: enemy.Id);

        _sut.IsSkillOnCooldown(tower.Id, skill.Id).Should().BeTrue();
    }

    [Fact]
    public void GetRemainingCooldown_AfterActivation_ReturnsCooldownMs()
    {
        var tower = CreateTestTower();
        var skill = CreateTargetedDamageSkill(cooldownMs: 5000);
        var enemy = CreateTestUnit(1, new Vector2(120, 100));
        _sut.RegisterSkill(tower.Id, skill);

        _sut.ActivateSkill(tower.Id, skill.Id, tower, new List<Unit> { enemy }, targetUnitId: enemy.Id);

        _sut.GetRemainingCooldown(tower.Id, skill.Id).Should().Be(5000);
    }

    [Fact]
    public void UpdateCooldowns_DecreasesRemainingCooldown()
    {
        var tower = CreateTestTower();
        var skill = CreateTargetedDamageSkill(cooldownMs: 5000);
        var enemy = CreateTestUnit(1, new Vector2(120, 100));
        _sut.RegisterSkill(tower.Id, skill);
        _sut.ActivateSkill(tower.Id, skill.Id, tower, new List<Unit> { enemy }, targetUnitId: enemy.Id);

        _sut.UpdateCooldowns(2000);

        _sut.GetRemainingCooldown(tower.Id, skill.Id).Should().Be(3000);
    }

    [Fact]
    public void UpdateCooldowns_CooldownReachesZero_SkillAvailable()
    {
        var tower = CreateTestTower();
        var skill = CreateTargetedDamageSkill(cooldownMs: 3000);
        var enemy = CreateTestUnit(1, new Vector2(120, 100));
        _sut.RegisterSkill(tower.Id, skill);
        _sut.ActivateSkill(tower.Id, skill.Id, tower, new List<Unit> { enemy }, targetUnitId: enemy.Id);

        _sut.UpdateCooldowns(3000);

        _sut.IsSkillOnCooldown(tower.Id, skill.Id).Should().BeFalse();
        _sut.GetRemainingCooldown(tower.Id, skill.Id).Should().Be(0);
    }

    [Fact]
    public void UpdateCooldowns_ForSpecificTower_OnlyAffectsThatTower()
    {
        var tower1 = CreateTestTower(id: 1);
        var tower2 = CreateTestTower(id: 2);
        var skill1 = CreateTargetedDamageSkill("skill1", cooldownMs: 5000);
        var skill2 = CreateTargetedDamageSkill("skill2", cooldownMs: 5000);
        var enemy1 = CreateTestUnit(1, new Vector2(120, 100), hp: 500);
        var enemy2 = CreateTestUnit(2, new Vector2(130, 100), hp: 500);
        _sut.RegisterSkill(tower1.Id, skill1);
        _sut.RegisterSkill(tower2.Id, skill2);
        _sut.ActivateSkill(tower1.Id, skill1.Id, tower1, new List<Unit> { enemy1 }, targetUnitId: enemy1.Id);
        _sut.ActivateSkill(tower2.Id, skill2.Id, tower2, new List<Unit> { enemy2 }, targetUnitId: enemy2.Id);

        _sut.UpdateCooldowns(tower1.Id, 2000);

        _sut.GetRemainingCooldown(tower1.Id, skill1.Id).Should().Be(3000);
        _sut.GetRemainingCooldown(tower2.Id, skill2.Id).Should().Be(5000);
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void ClearSkills_RemovesAllSkillsForTower()
    {
        var tower = CreateTestTower();
        _sut.RegisterSkill(tower.Id, CreateTargetedDamageSkill("skill1"));
        _sut.RegisterSkill(tower.Id, CreateAreaSkill("skill2"));

        _sut.ClearSkills(tower.Id);

        _sut.GetSkills(tower.Id).Should().BeEmpty();
    }

    [Fact]
    public void ClearAllSkills_RemovesEverything()
    {
        _sut.RegisterSkill(1, CreateTargetedDamageSkill("skill1"));
        _sut.RegisterSkill(2, CreateAreaSkill("skill2"));

        _sut.ClearAllSkills();

        _sut.GetSkills(1).Should().BeEmpty();
        _sut.GetSkills(2).Should().BeEmpty();
    }

    #endregion
}
