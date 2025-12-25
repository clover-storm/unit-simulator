using System.Numerics;
using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace UnitSimulator.Core.Tests.Units;

public class UnitRegistryTests
{
    [Fact]
    public void CreateWithDefaults_ShouldContainStandardUnits()
    {
        // Arrange & Act
        var registry = UnitRegistry.CreateWithDefaults();

        // Assert
        registry.HasDefinition("skeleton").Should().BeTrue();
        registry.HasDefinition("golemite").Should().BeTrue();
        registry.HasDefinition("lava_pup").Should().BeTrue();
        registry.HasDefinition("minion").Should().BeTrue();
        registry.HasDefinition("bat").Should().BeTrue();
    }

    [Fact]
    public void GetDefinition_ExistingUnit_ShouldReturnCorrectStats()
    {
        // Arrange
        var registry = UnitRegistry.CreateWithDefaults();

        // Act
        var skeleton = registry.GetDefinition("skeleton");

        // Assert
        skeleton.Should().NotBeNull();
        skeleton!.UnitId.Should().Be("skeleton");
        skeleton.MaxHP.Should().Be(81);
        skeleton.Damage.Should().Be(81);
        skeleton.Layer.Should().Be(MovementLayer.Ground);
        skeleton.Role.Should().Be(UnitRole.Melee);
    }

    [Fact]
    public void GetDefinition_NonExistingUnit_ShouldReturnNull()
    {
        // Arrange
        var registry = UnitRegistry.CreateWithDefaults();

        // Act
        var unknown = registry.GetDefinition("unknown_unit");

        // Assert
        unknown.Should().BeNull();
    }

    [Fact]
    public void Register_CustomDefinition_ShouldBeRetrievable()
    {
        // Arrange
        var registry = new UnitRegistry();
        var customDef = new UnitDefinition
        {
            UnitId = "custom_minion",
            DisplayName = "Custom Minion",
            MaxHP = 500,
            Damage = 100,
            Layer = MovementLayer.Air
        };

        // Act
        registry.Register(customDef);
        var retrieved = registry.GetDefinition("custom_minion");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.MaxHP.Should().Be(500);
        retrieved.Layer.Should().Be(MovementLayer.Air);
    }

    [Fact]
    public void CreateUnit_ShouldApplyDefinitionStats()
    {
        // Arrange
        var registry = UnitRegistry.CreateWithDefaults();
        var golemiteDef = registry.GetDefinition("golemite")!;

        // Act
        var unit = golemiteDef.CreateUnit(1, UnitFaction.Enemy, new Vector2(100, 100));

        // Assert
        unit.HP.Should().Be(900);  // golemite MaxHP
        unit.Damage.Should().Be(50);
        unit.Layer.Should().Be(MovementLayer.Ground);
        unit.Faction.Should().Be(UnitFaction.Enemy);
        unit.Position.Should().Be(new Vector2(100, 100));
    }

    [Fact]
    public void CreateUnit_WithAbilities_ShouldIncludeAbilities()
    {
        // Arrange
        var registry = UnitRegistry.CreateWithDefaults();
        var golemiteDef = registry.GetDefinition("golemite")!;

        // Act
        var unit = golemiteDef.CreateUnit(1, UnitFaction.Enemy, Vector2.Zero);

        // Assert: golemite has DeathDamage ability
        var deathDamage = unit.GetAbility<DeathDamageData>();
        deathDamage.Should().NotBeNull();
        deathDamage!.Damage.Should().Be(100);
        deathDamage.Radius.Should().Be(40f);
    }

    [Fact]
    public void LavaPup_ShouldBeAirUnit()
    {
        // Arrange
        var registry = UnitRegistry.CreateWithDefaults();
        var lavaPupDef = registry.GetDefinition("lava_pup")!;

        // Act
        var unit = lavaPupDef.CreateUnit(1, UnitFaction.Friendly, Vector2.Zero);

        // Assert
        unit.Layer.Should().Be(MovementLayer.Air);
        unit.CanTarget.Should().Be(TargetType.GroundAndAir);
        unit.Role.Should().Be(UnitRole.Ranged);
    }

    [Fact]
    public void ElixirGolemite_ShouldHaveChainedDeathSpawn()
    {
        // Arrange
        var registry = UnitRegistry.CreateWithDefaults();
        var elixirGolemiteDef = registry.GetDefinition("elixir_golemite")!;

        // Act
        var unit = elixirGolemiteDef.CreateUnit(1, UnitFaction.Enemy, Vector2.Zero);
        var deathSpawn = unit.GetAbility<DeathSpawnData>();

        // Assert: elixir_golemite spawns elixir_blob on death
        deathSpawn.Should().NotBeNull();
        deathSpawn!.SpawnUnitId.Should().Be("elixir_blob");
        deathSpawn.SpawnCount.Should().Be(2);

        // elixir_blob should also be registered
        registry.HasDefinition("elixir_blob").Should().BeTrue();
    }
}
