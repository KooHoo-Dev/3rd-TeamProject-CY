using CY.Core;
using UnityEngine;

namespace CY.Network
{
    /// <summary>비호스트용. 서버에서 받은 값을 보관만 한다.</summary>
    public class NetworkRoundStatus : MonoBehaviour, IRoundStatusProvider
    {
        public float RemainingTime { get; private set; }
        public float DurabilityRatio { get; private set; } = 1f;
        public int ClearedCheckpoints { get; private set; }
        public int TotalCheckpoints { get; private set; }
        public RoundPhase Phase { get; private set; } = RoundPhase.Waiting;

        public void Apply(float remainingTime, float durabilityRatio, int cleared, int total, RoundPhase phase)
        {
            RemainingTime = remainingTime;
            DurabilityRatio = durabilityRatio;
            ClearedCheckpoints = cleared;
            TotalCheckpoints = total;
            Phase = phase;
            
            if (durabilityRatio < 0f || durabilityRatio > 1f)
                Debug.LogWarning($"[Net] 내구도 비율 범위 밖: {durabilityRatio}");
        }
    }
}