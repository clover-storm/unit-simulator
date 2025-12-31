using FluentAssertions;
using ReferenceModels.Infrastructure;
using ReferenceModels.Models.Enums;
using ReferenceModels.Validation;

namespace ReferenceModels.Tests.Integration;

/// <summary>
/// 실제 JSON 파일 로드 및 참조 무결성 검증 테스트
/// </summary>
public class JsonLoadingTests
{
    private readonly string _dataPath;
    private readonly ReferenceManager _manager;

    public JsonLoadingTests()
    {
        // 프로젝트 루트 기준 data/references 경로 찾기
        var currentDir = Directory.GetCurrentDirectory();
        var projectRoot = FindProjectRoot(currentDir);
        _dataPath = Path.Combine(projectRoot, "data", "references");

        _manager = ReferenceManager.CreateWithDefaultHandlers();
        _manager.LoadAll(_dataPath, msg => Console.WriteLine(msg));
    }

    private string FindProjectRoot(string startPath)
    {
        var dir = new DirectoryInfo(startPath);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "data")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException("Could not find project root containing 'data' directory");
    }

    [Fact]
    public void LoadAll_ShouldLoadAllTables()
    {
        // Assert
        _manager.Units.Should().NotBeNull("units.json should be loaded");
        _manager.Skills.Should().NotBeNull("skills.json should be loaded");
        _manager.Buildings.Should().NotBeNull("buildings.json should be loaded");
        _manager.Spells.Should().NotBeNull("spells.json should be loaded");
        _manager.Towers.Should().NotBeNull("towers.json should be loaded");
    }

    [Fact]
    public void Units_ShouldContainExpectedUnits()
    {
        // Assert
        _manager.Units!.Get("knight").Should().NotBeNull();
        _manager.Units.Get("prince").Should().NotBeNull();
        _manager.Units.Get("baby_dragon").Should().NotBeNull();
        _manager.Units.Get("golem").Should().NotBeNull();
        _manager.Units.Get("skeleton").Should().NotBeNull();
    }

    [Fact]
    public void Knight_ShouldHaveCorrectData()
    {
        // Act
        var knight = _manager.Units!.Get("knight");

        // Assert
        knight.Should().NotBeNull();
        knight!.DisplayName.Should().Be("Knight");
        knight.EntityType.Should().Be(EntityType.Troop);
        knight.Role.Should().Be(UnitRole.MiniTank);
        knight.AttackType.Should().Be(AttackType.Melee);
        knight.AttackSpeed.Should().Be(1.1f);
        knight.MaxHP.Should().Be(1938);
    }

    [Fact]
    public void Prince_ShouldHaveCorrectData()
    {
        // Act
        var prince = _manager.Units!.Get("prince");

        // Assert
        prince.Should().NotBeNull();
        prince!.Role.Should().Be(UnitRole.GlassCannon);
        prince.AttackType.Should().Be(AttackType.MeleeMedium);
        prince.AttackSpeed.Should().Be(1.4f);
        prince.Skills.Should().Contain("prince_charge");
    }

    [Fact]
    public void BabyDragon_ShouldHaveCorrectData()
    {
        // Act
        var babyDragon = _manager.Units!.Get("baby_dragon");

        // Assert
        babyDragon.Should().NotBeNull();
        babyDragon!.Role.Should().Be(UnitRole.Support);
        babyDragon.AttackType.Should().Be(AttackType.Ranged);
        babyDragon.AttackSpeed.Should().Be(1.5f);
        babyDragon.Skills.Should().Contain("baby_dragon_splash");
    }

    [Fact]
    public void Golem_ShouldHaveCorrectData()
    {
        // Act
        var golem = _manager.Units!.Get("golem");

        // Assert
        golem.Should().NotBeNull();
        golem!.Role.Should().Be(UnitRole.Tank);
        golem.AttackType.Should().Be(AttackType.Melee);
        golem.AttackSpeed.Should().Be(2.5f);
        golem.CanTarget.Should().Be(TargetType.Building);
        golem.TargetPriority.Should().Be(TargetPriority.Buildings);
        golem.Skills.Should().Contain("golem_death_spawn");
        golem.Skills.Should().Contain("golem_death_damage");
    }

    [Fact]
    public void Skeleton_ShouldHaveCorrectData()
    {
        // Act
        var skeleton = _manager.Units!.Get("skeleton");

        // Assert
        skeleton.Should().NotBeNull();
        skeleton!.Role.Should().Be(UnitRole.Swarm);
        skeleton.AttackType.Should().Be(AttackType.MeleeShort);
        skeleton.SpawnCount.Should().Be(3);
    }

    [Fact]
    public void Buildings_ShouldContainExpectedBuildings()
    {
        // Assert
        _manager.Buildings!.Get("tombstone").Should().NotBeNull();
        _manager.Buildings.Get("cannon").Should().NotBeNull();
        _manager.Buildings.Get("tesla").Should().NotBeNull();
    }

    [Fact]
    public void Tombstone_ShouldHaveCorrectData()
    {
        // Act
        var tombstone = _manager.Buildings!.Get("tombstone");

        // Assert
        tombstone.Should().NotBeNull();
        tombstone!.Type.Should().Be(BuildingType.Spawner);
        tombstone.SpawnUnitId.Should().Be("skeleton");
        tombstone.SpawnCount.Should().Be(4);
        tombstone.SpawnInterval.Should().Be(2.9f);
        tombstone.FirstSpawnDelay.Should().Be(3.0f);
        tombstone.Lifetime.Should().Be(40.0f);
    }

    [Fact]
    public void Cannon_ShouldHaveCorrectData()
    {
        // Act
        var cannon = _manager.Buildings!.Get("cannon");

        // Assert
        cannon.Should().NotBeNull();
        cannon!.Type.Should().Be(BuildingType.Defensive);
        cannon.Damage.Should().Be(243);
        cannon.AttackSpeed.Should().Be(0.8f);
        cannon.AttackRange.Should().Be(5.5f);
        cannon.CanTarget.Should().Be(TargetType.Ground);
    }

    [Fact]
    public void Spells_ShouldContainExpectedSpells()
    {
        // Assert
        _manager.Spells!.Get("fireball").Should().NotBeNull();
        _manager.Spells.Get("zap").Should().NotBeNull();
        _manager.Spells.Get("poison").Should().NotBeNull();
        _manager.Spells.Get("freeze").Should().NotBeNull();
        _manager.Spells.Get("rage").Should().NotBeNull();
        _manager.Spells.Get("graveyard").Should().NotBeNull();
    }

    [Fact]
    public void Fireball_ShouldHaveCorrectData()
    {
        // Act
        var fireball = _manager.Spells!.Get("fireball");

        // Assert
        fireball.Should().NotBeNull();
        fireball!.Type.Should().Be(SpellType.Instant);
        fireball.Damage.Should().Be(572);
        fireball.BuildingDamageMultiplier.Should().Be(0.4f);
        fireball.Radius.Should().Be(2.5f);
    }

    [Fact]
    public void Zap_ShouldHaveCorrectData()
    {
        // Act
        var zap = _manager.Spells!.Get("zap");

        // Assert
        zap.Should().NotBeNull();
        zap!.Type.Should().Be(SpellType.Instant);
        zap.Damage.Should().Be(159);
        zap.AppliedEffect.Should().Be(StatusEffectType.Stunned);
        // Zap의 상태 효과 지속 시간은 스펠이 아닌 효과 자체에 정의됨
    }

    [Fact]
    public void Poison_ShouldHaveCorrectData()
    {
        // Act
        var poison = _manager.Spells!.Get("poison");

        // Assert
        poison.Should().NotBeNull();
        poison!.Type.Should().Be(SpellType.AreaOverTime);
        poison.Duration.Should().Be(8.0f);
        poison.DamagePerTick.Should().Be(95);
        poison.TickInterval.Should().Be(1.0f);
        poison.AppliedEffect.Should().Be(StatusEffectType.Slowed);
    }

    [Fact]
    public void Towers_ShouldContainExpectedTowers()
    {
        // Assert
        _manager.Towers!.Get("princess_tower").Should().NotBeNull();
        _manager.Towers.Get("king_tower").Should().NotBeNull();
    }

    [Fact]
    public void PrincessTower_ShouldHaveCorrectData()
    {
        // Act
        var tower = _manager.Towers!.Get("princess_tower");

        // Assert
        tower.Should().NotBeNull();
        tower!.Type.Should().Be(TowerType.Princess);
        tower.MaxHP.Should().Be(2534);
        tower.Damage.Should().Be(90);
        tower.AttackSpeed.Should().Be(0.8f);
        tower.AttackRadius.Should().Be(7.5f);
    }

    [Fact]
    public void ReferenceIntegrity_UnitSkills_ShouldExist()
    {
        // Arrange
        var unitsWithSkills = _manager.Units!.GetAll()
            .Where(u => u.Skills.Any())
            .ToList();

        // Act & Assert
        foreach (var unit in unitsWithSkills)
        {
            foreach (var skillId in unit.Skills)
            {
                _manager.Skills!.Get(skillId).Should().NotBeNull(
                    $"Unit '{unit.DisplayName}' references skill '{skillId}' which should exist");
            }
        }
    }

    [Fact]
    public void ReferenceIntegrity_BuildingSpawnUnits_ShouldExist()
    {
        // Arrange
        var spawnerBuildings = _manager.Buildings!.GetAll()
            .Where(b => b.Type == BuildingType.Spawner && !string.IsNullOrWhiteSpace(b.SpawnUnitId))
            .ToList();

        // Act & Assert
        foreach (var building in spawnerBuildings)
        {
            _manager.Units!.Get(building.SpawnUnitId!).Should().NotBeNull(
                $"Building '{building.DisplayName}' spawns unit '{building.SpawnUnitId}' which should exist");
        }
    }

    [Fact]
    public void ReferenceIntegrity_SpellSpawnUnits_ShouldExist()
    {
        // Arrange
        var spawningSpells = _manager.Spells!.GetAll()
            .Where(s => s.Type == SpellType.Spawning && !string.IsNullOrWhiteSpace(s.SpawnUnitId))
            .ToList();

        // Act & Assert
        foreach (var spell in spawningSpells)
        {
            _manager.Units!.Get(spell.SpawnUnitId!).Should().NotBeNull(
                $"Spell '{spell.DisplayName}' spawns unit '{spell.SpawnUnitId}' which should exist");
        }
    }

    [Fact]
    public void Validation_AllUnits_ShouldPassValidation()
    {
        // Arrange
        var validator = new UnitReferenceValidator();

        // Act & Assert
        foreach (var (id, unit) in _manager.Units!.GetAll().Select((u, i) => (id: _manager.Units.Keys.ElementAt(i), unit: u)))
        {
            var result = validator.Validate(unit, id);
            result.IsValid.Should().BeTrue(
                $"Unit '{id}' should pass validation. Errors: {string.Join(", ", result.Errors)}");
        }
    }

    [Fact]
    public void Validation_AllSkills_ShouldPassValidation()
    {
        // Arrange
        var validator = new SkillReferenceValidator();

        // Act & Assert
        foreach (var (id, skill) in _manager.Skills!.GetAll().Select((s, i) => (id: _manager.Skills.Keys.ElementAt(i), skill: s)))
        {
            var result = validator.Validate(skill, id);
            result.IsValid.Should().BeTrue(
                $"Skill '{id}' should pass validation. Errors: {string.Join(", ", result.Errors)}");
        }
    }

    [Fact]
    public void Validation_AllBuildings_ShouldPassValidation()
    {
        // Arrange
        var validator = new BuildingReferenceValidator();

        // Act & Assert
        foreach (var (id, building) in _manager.Buildings!.GetAll().Select((b, i) => (id: _manager.Buildings.Keys.ElementAt(i), building: b)))
        {
            var result = validator.Validate(building, id);
            result.IsValid.Should().BeTrue(
                $"Building '{id}' should pass validation. Errors: {string.Join(", ", result.Errors)}");
        }
    }

    [Fact]
    public void Validation_AllSpells_ShouldPassValidation()
    {
        // Arrange
        var validator = new SpellReferenceValidator();

        // Act & Assert
        foreach (var (id, spell) in _manager.Spells!.GetAll().Select((s, i) => (id: _manager.Spells.Keys.ElementAt(i), spell: s)))
        {
            var result = validator.Validate(spell, id);
            result.IsValid.Should().BeTrue(
                $"Spell '{id}' should pass validation. Errors: {string.Join(", ", result.Errors)}");
        }
    }

    [Fact]
    public void Validation_AllTowers_ShouldPassValidation()
    {
        // Arrange
        var validator = new TowerReferenceValidator();

        // Act & Assert
        foreach (var (id, tower) in _manager.Towers!.GetAll().Select((t, i) => (id: _manager.Towers.Keys.ElementAt(i), tower: t)))
        {
            var result = validator.Validate(tower, id);
            result.IsValid.Should().BeTrue(
                $"Tower '{id}' should pass validation. Errors: {string.Join(", ", result.Errors)}");
        }
    }
}
