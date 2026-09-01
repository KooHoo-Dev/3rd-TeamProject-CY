using System;
using CY.Network;
using Cysharp.Threading.Tasks;
using HelloServer;
using Newtonsoft.Json;
using UnityEngine;

namespace CY.Core
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
        
        // ───────────────── 송신 (호스트)

        private void Update()
        {
            if (Session.IsHost == false) return;
            if (hostStatus == null) return;
            if (ShouldSend() == false) return;

            Send();
        }
        
        /// <summary>값이 바뀌었거나 주기가 됐으면 보낸다.</summary>
        private bool ShouldSend()
        {
            if (hostStatus.Phase != lastPhase) return true;
            if (hostStatus.ClearedCheckpoints != lastCleared) return true;
            if (Mathf.Abs(hostStatus.DurabilityRatio - lastDurability) > durabilityEpsilon) return true;

            return Time.unscaledTime >= nextSendAt;
        }
        
        private void Send()
        {
            lastPhase = hostStatus.Phase;
            lastCleared = hostStatus.ClearedCheckpoints;
            lastDurability = hostStatus.DurabilityRatio;
            nextSendAt = Time.unscaledTime + 1f / Mathf.Max(1f, sendPerSecond);

            if (session == null) return;

            session.SendAsync(new RoundStateMessage
            {
                RemainingTime = hostStatus.RemainingTime,
                DurabilityRatio = hostStatus.DurabilityRatio,
                Cleared = hostStatus.ClearedCheckpoints,
                Total = hostStatus.TotalCheckpoints,
                Phase = hostStatus.Phase.ToString(),
            }).Forget();
        }
        
        // ───────────────── 수신 (비호스트)

        private void HandleMessage(string type, string json)
        {
            if (type != "round_state") return;
            if (Session.IsHost) return; // 호스트는 자기 값을 쓴다
            if (networkStatus == null) return;

            RoundStateMessage m = JsonConvert.DeserializeObject<RoundStateMessage>(json);
            if (m == null) return;

            if (Enum.TryParse(m.Phase, out RoundPhase phase) == false)
                phase = RoundPhase.Waiting;

            if (m.DurabilityRatio < 0f || m.DurabilityRatio > 1f)
                Debug.LogWarning($"[RoundState] 내구도 비율 범위 밖: {m.DurabilityRatio}", this);

            networkStatus.Apply(m.RemainingTime, m.DurabilityRatio, m.Cleared, m.Total, phase);
        }
    }
}