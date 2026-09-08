using System;
using CY.Core;
using CY.Network;
using UnityEngine;
using UnityEngine.Events;

namespace CY.Vehicle
{
    [Serializable]
    public class DurabilityChangedEvent : UnityEvent<float> { }

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(VehicleController))]
    public class VehicleDamage : MonoBehaviour
    {
        public static event Action OnMiddleCrash;
        public static event Action OnHardCrash;
        public event Action Destroyed;
        
        [Header("내구도")] [SerializeField] private float maxDurability = 100f;
        [Header("충돌 데미지")] 
        [SerializeField] private float ignoreSpeed = 3f;                   
        [SerializeField] private float hardCrashSpeedThreshold = 10f;       
        [SerializeField] private float damagePerSpeed = 4f;                

        [Header("속도 페널티 (3단계)")] 
        [SerializeField, Range(0f, 1f)] private float damagedThreshold = 0.4f;
        [SerializeField, Range(0f, 1f)] private float severeThreshold = 0.2f;
        [SerializeField, Range(0f, 1f)] private float damagedMultiplier = 0.6f;
        [SerializeField, Range(0f, 1f)] private float severeMultiplier = 0.3f;

        [Header("내구도 변경 신호")] [SerializeField] private DurabilityChangedEvent onDurabilityChanged;

        public float Durability => durability;
        public float MaxDurability => maxDurability;
        public float DurabilityRatio => durability / maxDurability;
        public bool IsDestroyed => durability <= 0f;

        private float durability;
        private VehicleController controller;
        private RoundManager roundManager;

        private void Awake()
        {
            controller = GetComponent<VehicleController>();
            ResetDurability();
        }
        
        // [추가] 인게임에서 차가 켜질 때 네트워크 구독
        private void OnEnable()
        {
            if (NetworkSession.Instance != null)
                NetworkSession.Instance.MessageReceived += OnNetworkMessageReceived;
        }

        // [추가] 차가 꺼질 때 네트워크 구독 해제
        private void OnDisable()
        {
            if (NetworkSession.Instance != null)
                NetworkSession.Instance.MessageReceived -= OnNetworkMessageReceived;
        }

        // [추가] 비호스트 클라이언트가 충돌 패킷을 받아서 자기 차에서 소리 이벤트를 발생시킴
        private void OnNetworkMessageReceived(string type, string json)
        {
            if (Session.IsHost) return;

            if (type == "vehicle_crash")
            {
                if (json.Contains("\"isHard\":true") || json.Contains("\"isHard\": true"))
                    OnHardCrash?.Invoke();
                else
                    OnMiddleCrash?.Invoke();
            }
        }

        public void Bind(RoundManager manager) => roundManager = manager;

        public void ResetDurability()
        {
            durability = maxDurability;
            ApplySpeedPenalty();
            onDurabilityChanged.Invoke(DurabilityRatio);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (IsDestroyed) return;
    
            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed < ignoreSpeed) return;
    
            bool isHard = impactSpeed >= hardCrashSpeedThreshold;

            if (isHard) OnHardCrash?.Invoke();
            else OnMiddleCrash?.Invoke();
 
            // 데미지 처리 및 네트워크 송신은 호스트만
            if (!Session.IsHost) return;

            // 호스트가 비호스트들에게 충돌 발생 알림
            _ = NetworkSession.Instance?.SendAsync
                (new { type = "vehicle_crash", isHard = isHard });

            float damage = (impactSpeed - ignoreSpeed) * damagePerSpeed;
            durability = Mathf.Max(0f, durability - damage);

            ApplySpeedPenalty();
            onDurabilityChanged.Invoke(DurabilityRatio);

            if (IsDestroyed)
            {
                Destroyed?.Invoke();
                roundManager?.FailRound();
            }
        }

        private void ApplySpeedPenalty()
        {
            float ratio = DurabilityRatio;
            float multiplier = ratio > damagedThreshold ? 1f : ratio > severeThreshold ? damagedMultiplier : severeMultiplier;
            controller.SetSpeedMultiplier(multiplier);
        }
    }
}