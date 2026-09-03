using System;
using CY.Core;
using Cysharp.Threading.Tasks;
using HelloServer;
using Newtonsoft.Json;
using UnityEngine;
using CY.Vehicle;

namespace CY.Network
{
    public class RoundStateSync : MonoBehaviour
    {
        [Header("상태 공급자")]
        [SerializeField] private HostRoundStatus hostStatus;
        [SerializeField] private NetworkRoundStatus networkStatus;

        [Header("전송")]
        [SerializeField] private float sendPerSecond = 5f;
        [SerializeField] private float durabilityEpsilon = 0.001f;

        private NetworkSession session;
        
        private float nextSendAt;
        private RoundPhase lastPhase = RoundPhase.Waiting;
        private int lastCleared = -1;
        private float lastDurability = -1f;
        
        private Gear lastGear = Gear.N;
        
        private void Awake()
        {
            if (hostStatus == null) hostStatus = FindAnyObjectByType<HostRoundStatus>();
            if (networkStatus == null) networkStatus = FindAnyObjectByType<NetworkRoundStatus>();

            session = NetworkSession.Instance;
        }
        
        private void OnEnable()
        {
            if (session != null) session.MessageReceived += HandleMessage;
        }

        private void OnDisable()
        {
            if (session != null) session.MessageReceived -= HandleMessage;
        }
        
        #region 호스트에서 보낼 때
        private void Update()
        {
            if (Session.IsHost == false) return;
            if (hostStatus == null) return;
            if (ShouldSend() == false) return;

            Send();
        }
        
        private bool ShouldSend()
        {
            if (hostStatus.Phase != lastPhase) return true;
            if (hostStatus.ClearedCheckpoints != lastCleared) return true;
            if (Mathf.Abs(hostStatus.DurabilityRatio - lastDurability) > durabilityEpsilon) return true;
            if (hostStatus.CurrentGear != lastGear) return true;

            return Time.unscaledTime >= nextSendAt;
        }
        
        private void Send()
        {
            lastPhase = hostStatus.Phase;
            lastCleared = hostStatus.ClearedCheckpoints;
            lastDurability = hostStatus.DurabilityRatio;
            nextSendAt = Time.unscaledTime + 1f / Mathf.Max(1f, sendPerSecond);
            lastGear = hostStatus.CurrentGear;

            if (session == null) return;

            session.SendAsync(new RoundStateMessage
            {
                RemainingTime = hostStatus.RemainingTime,
                DurabilityRatio = hostStatus.DurabilityRatio,
                SpeedRatio = hostStatus.SpeedRatio,
                Cleared = hostStatus.ClearedCheckpoints,
                Total = hostStatus.TotalCheckpoints,
                Phase = hostStatus.Phase.ToString(),
                Gear = hostStatus.CurrentGear.ToString(),
            }).Forget();
        }
        #endregion
        
        #region 호스트가 아닌 유저가 받을 때
        private void HandleMessage(string type, string json)
        {
            if (type != "round_state") return;
            if (Session.IsHost) return; // 호스트는 자기 값을 쓴다
            if (networkStatus == null) return;

            RoundStateMessage m = JsonConvert.DeserializeObject<RoundStateMessage>(json);
            if (m == null) return;

            if (Enum.TryParse(m.Phase, out RoundPhase phase) == false
                || Enum.IsDefined(typeof(RoundPhase), phase) == false)
            {
                phase = RoundPhase.Waiting;
            }
            
            if (Enum.TryParse(m.Gear, out Gear gear) == false
                || Enum.IsDefined(typeof(Gear), gear) == false)
            {
                gear = Gear.N;
            }

            if (m.DurabilityRatio < 0f || m.DurabilityRatio > 1f)
                Debug.LogWarning($"[RoundState] 내구도 비율 범위 밖: {m.DurabilityRatio}", this);

            networkStatus.Apply(m.RemainingTime, m.DurabilityRatio, m.SpeedRatio, m.Cleared, m.Total, phase, gear);
        }
        #endregion
    }
}