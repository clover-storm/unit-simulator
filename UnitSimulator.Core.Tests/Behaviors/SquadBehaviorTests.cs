using System.Numerics;
using UnitSimulator.Core.Tests.TestHelpers;
using Xunit;

namespace UnitSimulator.Core.Tests.Behaviors;

public class SquadBehaviorTests
{
    [Fact]
    public void UpdateFriendlySquad_NoEnemies_MovesLeaderTowardMainTarget()
    {
        var behavior = new SquadBehavior();
        var leader = new Unit(new Vector2(0, 0), 20f, 4.5f, 0.08f, UnitRole.Melee, 100, 1, UnitFaction.Friendly);
        var follower = new Unit(new Vector2(0, 50), 20f, 4.5f, 0.08f, UnitRole.Ranged, 100, 2, UnitFaction.Friendly);
        var friendlies = new List<Unit> { leader, follower };
        var enemies = new List<Unit>();
        var enemyTowers = new List<Tower>();

        var simulator = SimulationTestFactory.CreateInitializedCore(hasMoreWaves: false);
        var events = new FrameEvents();

        behavior.UpdateFriendlySquad(simulator, friendlies, enemies, enemyTowers, new Vector2(300, 300), events);

        Assert.Equal(new Vector2(300, 300), leader.CurrentDestination);
    }
}
