using CY.Vehicle;

namespace CY.Core
{
    /// <summary>HUD 가 읽는 라운드 상태. 호스트는 직접 계산하고, 비호스트는 서버 값을 받는다.</summary>
    public interface IRoundStatusProvider
    {
        float RemainingTime { get; }
        float DurabilityRatio { get; }   // 0 ~ 1
        float SpeedRatio { get; }
        int ClearedCheckpoints { get; }
        int TotalCheckpoints { get; }
        RoundPhase Phase { get; }
        Gear CurrentGear { get; }
    }
}