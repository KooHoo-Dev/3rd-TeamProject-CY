using CY.Vehicle;
using UnityEngine;

namespace CY.Core
{
    /// <summary>호스트용. 실제 시스템에서 값을 직접 읽는다.</summary>
    public class HostRoundStatus : MonoBehaviour, IRoundStatusProvider
    {
        [SerializeField] private RoundTimer timer;
        [SerializeField] private RoundManager roundManager;
        [SerializeField] private VehicleDamage damage;
        [SerializeField] private VehicleController controller;

        public float RemainingTime => timer != null ? timer.RemainingTime : 0f;
        public float DurabilityRatio => damage != null ? damage.DurabilityRatio : 1f;
        public float SpeedRatio      => controller != null ? controller.SpeedRatio : 0f;

        public int ClearedCheckpoints => roundManager != null ? roundManager.ClearedCount : 0;
        public int TotalCheckpoints   => roundManager != null ? roundManager.TotalCheckpoints : 0;

        public RoundPhase Phase => roundManager != null ? roundManager.Phase : RoundPhase.Waiting;
        public Gear CurrentGear => controller != null ? controller.LastInput.CurrentGear : Gear.N;
    }
}