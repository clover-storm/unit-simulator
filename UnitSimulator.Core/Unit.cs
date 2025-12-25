using System.Collections.Generic;
using System.Numerics;

namespace UnitSimulator;

public enum UnitRole { Melee, Ranged }
public enum UnitFaction { Friendly, Enemy }

/// <summary>
/// 유닛의 이동 레이어를 정의합니다.
/// Ground 유닛은 지형/장애물 영향을 받고, Air 유닛은 지형을 무시합니다.
/// </summary>
public enum MovementLayer
{
    Ground,  // 지상 유닛 - 지형/충돌 영향 받음
    Air      // 공중 유닛 - 지형 무시, 공중 유닛끼리만 충돌
}

/// <summary>
/// 유닛이 공격할 수 있는 대상 유형을 정의합니다.
/// Flags 속성으로 복수 선택 가능합니다.
/// </summary>
[Flags]
public enum TargetType
{
    None     = 0,
    Ground   = 1 << 0,  // 지상 유닛 공격 가능
    Air      = 1 << 1,  // 공중 유닛 공격 가능
    Building = 1 << 2,  // 건물 공격 가능

    GroundAndAir = Ground | Air,
    All = Ground | Air | Building
}

public class Unit
{
    public Vector2 Position { get; set; }
    public Vector2 Velocity { get; set; }
    public Vector2 Forward { get; set; }
    public float Radius { get; }
    public float Speed { get; }
    public float TurnSpeed { get; }
    public Unit? Target { get; set; }
    public int HP { get; set; }
    public UnitRole Role { get; }
    public float AttackRange { get; }
    public float AttackCooldown { get; set; }
    public bool IsDead { get; set; }
    public int Id { get; }
    public UnitFaction Faction { get; }
    public Vector2 CurrentDestination { get; set; } = Vector2.Zero;
    public Unit? AvoidanceThreat { get; set; }
    public string Label => $"{(Faction == UnitFaction.Friendly ? "F" : "E")}{Id}";
    public List<Tuple<Unit, int>> RecentAttacks { get; } = new();
    public Unit?[] AttackSlots { get; } = new Unit?[GameConstants.NUM_ATTACK_SLOTS];
    public int TakenSlotIndex { get; set; } = -1;
    public Vector2 AvoidanceTarget { get; set; } = Vector2.Zero;
    public bool HasAvoidanceTarget { get; set; }
    public int FramesSinceSlotEvaluation { get; set; }
    public int FramesSinceTargetEvaluation { get; set; }

    // Phase 1: Ground/Air Layer System
    /// <summary>
    /// 유닛의 이동 레이어 (Ground/Air)
    /// </summary>
    public MovementLayer Layer { get; }

    /// <summary>
    /// 유닛이 공격할 수 있는 대상 유형
    /// </summary>
    public TargetType CanTarget { get; }

    private readonly List<Vector2> _avoidancePath = new();
    private int _avoidancePathIndex = 0;

    private readonly List<Vector2> _movementPath = new();
    private int _movementPathIndex = 0;

    public Unit(Vector2 position, float radius, float speed, float turnSpeed, UnitRole role, int hp, int id, UnitFaction faction,
        MovementLayer layer = MovementLayer.Ground, TargetType canTarget = TargetType.Ground)
    {
        Position = position;
        CurrentDestination = position;
        Radius = radius;
        Speed = speed;
        TurnSpeed = turnSpeed;
        Role = role;
        HP = hp;
        AttackRange = (role == UnitRole.Melee) ? radius * GameConstants.MELEE_RANGE_MULTIPLIER : radius * GameConstants.RANGED_RANGE_MULTIPLIER;
        AttackCooldown = 0;
        IsDead = false;
        Velocity = Vector2.Zero;
        Forward = Vector2.UnitX;
        Target = null;
        Id = id;
        Faction = faction;
        Layer = layer;
        CanTarget = canTarget;
    }

    /// <summary>
    /// 이 유닛이 지정된 대상을 공격할 수 있는지 확인합니다.
    /// </summary>
    public bool CanAttack(Unit target)
    {
        if (target == null || target.IsDead) return false;

        // 대상의 레이어에 따라 TargetType 확인
        TargetType targetLayer = target.Layer == MovementLayer.Air ? TargetType.Air : TargetType.Ground;
        return (CanTarget & targetLayer) != TargetType.None;
    }

    /// <summary>
    /// 이 유닛이 지정된 대상과 같은 레이어에 있는지 확인합니다.
    /// (충돌 검사 등에 사용)
    /// </summary>
    public bool IsSameLayer(Unit other)
    {
        if (other == null) return false;
        return Layer == other.Layer;
    }

    public Vector2 GetSlotPosition(int slotIndex, float attackerRadius)
    {
        float angle = (2 * MathF.PI / GameConstants.NUM_ATTACK_SLOTS) * slotIndex;
        float distance = this.Radius + attackerRadius + 10f;
        return this.Position + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;
    }

    public int TryClaimSlot(Unit attacker)
    {
        for (int i = 0; i < GameConstants.NUM_ATTACK_SLOTS; i++)
        {
            if (AttackSlots[i] == null)
            {
                AttackSlots[i] = attacker;
                attacker.TakenSlotIndex = i;
                return i;
            }
        }
        return -1;
    }

    public int ClaimBestSlot(Unit attacker)
    {
        int bestIndex = -1;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < GameConstants.NUM_ATTACK_SLOTS; i++)
        {
            var occupant = AttackSlots[i];
            if (occupant != null && occupant != attacker) continue;

            float distance = Vector2.Distance(attacker.Position, GetSlotPosition(i, attacker.Radius));
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestIndex = i;
            }
        }

        if (bestIndex != -1)
        {
            if (attacker.TakenSlotIndex != -1 && attacker.TakenSlotIndex != bestIndex &&
                attacker.TakenSlotIndex < AttackSlots.Length && AttackSlots[attacker.TakenSlotIndex] == attacker)
            {
                AttackSlots[attacker.TakenSlotIndex] = null;
            }
            AttackSlots[bestIndex] = attacker;
            attacker.TakenSlotIndex = bestIndex;
        }
        else
        {
            ReleaseSlot(attacker);
        }

        return bestIndex;
    }

    public void ReleaseSlot(Unit attacker)
    {
        if (attacker.TakenSlotIndex != -1 && attacker.TakenSlotIndex < AttackSlots.Length)
        {
            if (AttackSlots[attacker.TakenSlotIndex] == attacker)
            {
                AttackSlots[attacker.TakenSlotIndex] = null;
            }
            attacker.TakenSlotIndex = -1;
        }
    }

    public void SetAvoidancePath(List<Vector2> waypoints)
    {
        _avoidancePath.Clear();
        if (waypoints.Count == 0)
        {
            _avoidancePathIndex = 0;
            return;
        }
        _avoidancePath.AddRange(waypoints);
        _avoidancePathIndex = 0;
    }

    public bool TryGetNextAvoidanceWaypoint(out Vector2 waypoint)
    {
        while (_avoidancePathIndex < _avoidancePath.Count)
        {
            var target = _avoidancePath[_avoidancePathIndex];
            if (Vector2.Distance(Position, target) <= GameConstants.AVOIDANCE_WAYPOINT_THRESHOLD)
            {
                _avoidancePathIndex++;
                continue;
            }
            waypoint = target;
            return true;
        }
        waypoint = Vector2.Zero;
        return false;
    }

    public void ClearAvoidancePath()
    {
        _avoidancePath.Clear();
        _avoidancePathIndex = 0;
    }

    public void SetMovementPath(List<Vector2>? path)
    {
        _movementPath.Clear();
        if (path != null && path.Count > 0)
        {
            _movementPath.AddRange(path);
        }
        _movementPathIndex = 0;
    }

    public bool TryGetNextMovementWaypoint(out Vector2 waypoint)
    {
        if (_movementPathIndex < _movementPath.Count)
        {
            var target = _movementPath[_movementPathIndex];
            if (Vector2.Distance(Position, target) <= GameConstants.AVOIDANCE_WAYPOINT_THRESHOLD)
            {
                _movementPathIndex++;
                if (_movementPathIndex >= _movementPath.Count)
                {
                    waypoint = Vector2.Zero;
                    return false; // Path complete
                }
            }
            waypoint = _movementPath[_movementPathIndex];
            return true;
        }
        waypoint = Vector2.Zero;
        return false;
    }

    public void ClearMovementPath()
    {
        _movementPath.Clear();
        _movementPathIndex = 0;
    }

    public void UpdateRotation()
    {
        if (Velocity.LengthSquared() < 0.001f) return;
        float targetAngle = MathF.Atan2(Velocity.Y, Velocity.X);
        float currentAngle = MathF.Atan2(Forward.Y, Forward.X);
        float angleDiff = targetAngle - currentAngle;
        while (angleDiff > MathF.PI) angleDiff -= 2 * MathF.PI;
        while (angleDiff < -MathF.PI) angleDiff += 2 * MathF.PI;
        float rotation = Math.Clamp(angleDiff, -TurnSpeed, TurnSpeed);
        Forward = Vector2.Transform(Forward, Matrix3x2.CreateRotation(rotation));
    }

    public void TakeDamage(int damage = 1)
    {
        HP = Math.Max(0, HP - damage);
        if (HP <= 0 && !IsDead)
        {
            IsDead = true;
            Velocity = Vector2.Zero;
            Target?.ReleaseSlot(this);
        }
    }
}