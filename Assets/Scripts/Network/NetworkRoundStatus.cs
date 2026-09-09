using CY.Core;
using CY.Vehicle;
using UnityEngine;

namespace CY.Network
{
    /// <summary>비호스트용. 서버에서 받은 값을 보관만 한다.</summary>
    public class NetworkRoundStatus : MonoBehaviour, IRoundStatusProvider
    {
        public float ElapsedTime { get; private set; }
        public float DurabilityRatio { get; private set; } = 1f;
        public float SpeedRatio { get; private set; }
        public int ClearedCheckpoints { get; private set; }
        public int TotalCheckpoints { get; private set; }
        public RoundPhase Phase { get; private set; } = RoundPhase.Waiting;
        public Gear CurrentGear { get; private set; } = Gear.N;

        public void Apply(
            float elapsedTime, float durabilityRatio, float speedRatio, int cleared, int total, RoundPhase phase,
            Gear gear
            )
        {
            ElapsedTime = elapsedTime;
            DurabilityRatio = durabilityRatio;
            SpeedRatio = speedRatio;
            ClearedCheckpoints = cleared;
            TotalCheckpoints = total;
            Phase = phase;
            CurrentGear = gear;
            
            if (durabilityRatio < 0f || durabilityRatio > 1f)
                Debug.LogWarning($"[Net] 내구도 비율 범위 밖: {durabilityRatio}");
        }
    }
}