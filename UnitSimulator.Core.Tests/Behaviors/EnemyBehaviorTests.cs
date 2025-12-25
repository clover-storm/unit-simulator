using System.Numerics;
using UnitSimulator.Core.Tests.TestHelpers;
using Xunit;

namespace UnitSimulator.Core.Tests.Behaviors;

public class EnemyBehaviorTests
{
    [Fact]
    public void UpdateEnemySquad_NoFriendlies_DoesNothing()
    {
        var behavior = new EnemyBehavior();
        var enemy = new Unit(new Vector2(100, 100), 20f, 4.0f, 0.1f, UnitRole.Melee, 10, 1, UnitFaction.Enemy);
        var enemies = new List<Unit> { enemy };
        var friendlies = new List<Unit>();
        var friendlyTowers = new List<Tower>();

        var simulator = SimulationTestFactory.CreateInitializedCore(hasMoreWaves: false);
        var events = new FrameEvents();

        behavior.UpdateEnemySquad(simulator, enemies, friendlies, friendlyTowers, events);

        Assert.Equal(new Vector2(100, 100), enemy.Position);
        Assert.Null(enemy.Target);
    }
}
