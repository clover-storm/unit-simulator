using System.Linq;
using System.Numerics;

namespace UnitSimulator;

public static class AvoidanceSystem
{
    public static Vector2 PredictiveAvoidanceVector(Unit mover, List<Unit> others, out Vector2 avoidanceTarget, out bool isDetouring, out Unit? avoidanceThreat)
    {
        avoidanceTarget = Vector2.Zero;
        avoidanceThreat = null;
        isDetouring = false;
        float moverRadius = mover.Radius * Constants.COLLISION_RADIUS_SCALE;
        float minSpeed = MathF.Max(mover.Speed, 0.001f);

        var risks = new List<(Vector2 relPos, float distance, float combinedRadius, Unit threat)>();

        foreach (var other in others)
        {
            if (other == mover || other.IsDead) continue;
            float combinedRadius = moverRadius + other.Radius * Constants.COLLISION_RADIUS_SCALE;

            Vector2 relativePos = other.Position - mover.Position;
            Vector2 relativeVel = other.Velocity - mover.Velocity;
            float relativeSpeedSq = relativeVel.LengthSquared();

            if (MathUtils.TryGetFirstCollision(mover, other, out var tCollision, out var _))
            {
                float collisionWindow = MathF.Min((combinedRadius * 2f) / minSpeed, Constants.AVOIDANCE_MAX_LOOKAHEAD);
                if (tCollision <= collisionWindow)
                {
                    Vector2 relAtCollision = (other.Position + other.Velocity * tCollision) - (mover.Position + mover.Velocity * tCollision);
                    float distanceAtCollision = relAtCollision.Length();
                    if (distanceAtCollision > 0.0001f)
                    {
                        risks.Add((relAtCollision, distanceAtCollision, combinedRadius, other));
                        continue;
                    }
                }
            }

            float tClosest = relativeSpeedSq < 0.0001f
                ? 0f
                : Math.Max(-Vector2.Dot(relativePos, relativeVel) / relativeSpeedSq, 0f);

            float timeWindow = MathF.Min((combinedRadius * 2f) / minSpeed, Constants.AVOIDANCE_MAX_LOOKAHEAD);

            Vector2 futureRelPos = relativePos + relativeVel * tClosest;
            float futureDistance = futureRelPos.Length();

            if (futureDistance < combinedRadius && tClosest <= timeWindow && futureDistance > 0.0001f)
            {
                risks.Add((relativePos, relativePos.Length(), combinedRadius, other));
                continue;
            }

            Vector2 heading = mover.Velocity.LengthSquared() > 0.0001f ? MathUtils.SafeNormalize(mover.Velocity) : mover.Forward;
            float projection = Vector2.Dot(relativePos, heading);
            float lookaheadDistance = mover.Speed * Constants.AVOIDANCE_MAX_LOOKAHEAD + combinedRadius;
            if (projection > 0 && projection <= lookaheadDistance)
            {
                Vector2 lateral = relativePos - heading * projection;
                if (lateral.Length() < combinedRadius)
                {
                    risks.Add((relativePos, projection, combinedRadius, other));
                }
            }
        }

        if (!risks.Any()) return Vector2.Zero;

        var primaryRisk = risks.OrderBy(r => r.distance).First();
        avoidanceThreat = primaryRisk.threat;

        Vector2 baseDir = mover.Velocity.LengthSquared() > 0.0001f ? MathUtils.SafeNormalize(mover.Velocity) : mover.Forward;
        float minDistance = risks.Min(r => r.distance);
        float desiredWeight = Math.Clamp(minDistance / (moverRadius + 0.001f), 1f, 3f);

        for (int i = 0; i <= Constants.MAX_AVOIDANCE_ITERATIONS; i++)
        {
            var offsets = i == 0 ? new float[] { 0f } : new float[] { Constants.AVOIDANCE_ANGLE_STEP * i, -Constants.AVOIDANCE_ANGLE_STEP * i };
            foreach (var angle in offsets)
            {
                Vector2 candidate = MathUtils.Rotate(baseDir, angle);
                if (IsDirectionClear(candidate, risks))
                {
                    avoidanceTarget = mover.Position + candidate * MathF.Max(minDistance, moverRadius * 2f);
                    isDetouring = MathF.Abs(angle) > 0.001f;
                    if (!isDetouring)
                    {
                        avoidanceTarget = Vector2.Zero; // do not visualize straight paths
                        avoidanceThreat = null;
                    }
                    return candidate * desiredWeight;
                }
            }
        }

        Vector2 away = MathUtils.SafeNormalize(-primaryRisk.relPos);
        avoidanceTarget = mover.Position + away * MathF.Max(primaryRisk.distance, moverRadius * 2f);
        isDetouring = true;
        avoidanceThreat = primaryRisk.threat;
        return away * desiredWeight;
    }

    private static bool IsDirectionClear(Vector2 direction, List<(Vector2 relPos, float distance, float combinedRadius, Unit threat)> risks)
    {
        foreach (var (relPos, distance, combinedRadius, _) in risks)
        {
            float projection = Vector2.Dot(relPos, direction);
            if (projection < 0 || projection > distance) continue;

            Vector2 lateral = relPos - direction * projection;
            if (lateral.Length() < combinedRadius) return false;
        }
        return true;
    }
}
