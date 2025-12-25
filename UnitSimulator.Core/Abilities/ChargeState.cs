using System.Numerics;

namespace UnitSimulator;

/// <summary>
/// Phase 2: 유닛의 돌진 상태를 추적
/// </summary>
public class ChargeState
{
    /// <summary>현재 돌진 중인지 여부</summary>
    public bool IsCharging { get; set; }

    /// <summary>돌진이 완료되었는지 (필요 거리 이동 완료)</summary>
    public bool IsCharged { get; set; }

    /// <summary>돌진 시작 위치</summary>
    public Vector2 ChargeStartPosition { get; set; }

    /// <summary>현재까지 이동한 돌진 거리</summary>
    public float ChargedDistance { get; set; }

    /// <summary>돌진 완료에 필요한 거리</summary>
    public float RequiredDistance { get; set; }

    /// <summary>돌진 상태 초기화</summary>
    public void Reset()
    {
        IsCharging = false;
        IsCharged = false;
        ChargeStartPosition = Vector2.Zero;
        ChargedDistance = 0f;
    }

    /// <summary>돌진 시작</summary>
    public void StartCharge(Vector2 position, float requiredDistance)
    {
        IsCharging = true;
        IsCharged = false;
        ChargeStartPosition = position;
        ChargedDistance = 0f;
        RequiredDistance = requiredDistance;
    }

    /// <summary>돌진 거리 업데이트</summary>
    public void UpdateChargeDistance(Vector2 currentPosition)
    {
        if (!IsCharging) return;

        ChargedDistance = Vector2.Distance(ChargeStartPosition, currentPosition);
        if (ChargedDistance >= RequiredDistance)
        {
            IsCharged = true;
        }
    }

    /// <summary>공격 후 돌진 상태 소비 (리셋)</summary>
    public void ConsumeCharge()
    {
        Reset();
    }
}
