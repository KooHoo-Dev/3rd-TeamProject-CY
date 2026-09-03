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
        // 사운드 매니저 구독용 static 이벤트 추가
        public static event Action OnCrash;
        
        [Header("내구도")] [SerializeField] private float maxDurability = 100f;

        [Header("충돌 데미지")] [SerializeField] private float ignoreSpeed = 3f; // 이 속도 미만 충돌은 무시
        [SerializeField] private float damagePerSpeed = 4f; // 초과 속도 1당 데미지

        [Header("속도 페널티 (3단계)")] [SerializeField, Range(0f, 1f)]
        private float damagedThreshold = 0.4f;

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

        public void Bind(RoundManager manager) => roundManager = manager;

        public void ResetDurability()
        {
            durability = maxDurability;
            ApplySpeedPenalty();
            onDurabilityChanged.Invoke(DurabilityRatio);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!Session.IsHost) return;
            if (IsDestroyed) return;
            
            // 충돌 직전의 상대 속도. 벽은 정지해 있으므로 사실상 차의 속도
            float impactSpeed = collision.relativeVelocity.magnitude;
            if (impactSpeed < ignoreSpeed) return;
            
            OnCrash?.Invoke();

            float damage = (impactSpeed - ignoreSpeed) * damagePerSpeed;
            durability = Mathf.Max(0f, durability - damage);

            ApplySpeedPenalty();
            onDurabilityChanged.Invoke(DurabilityRatio);

            Debug.Log($"[Damage] 충돌 속도 {impactSpeed:0.#} → 데미지 {damage:0.#} / 내구도 {durability:0.#}", this);

            if (IsDestroyed)
            {
                Debug.Log("[Damage] 차량 파괴 — 라운드 실패", this);
                roundManager?.FailRound();
            }
        }

        private void ApplySpeedPenalty()
        {
            float ratio = DurabilityRatio;

            float multiplier =
                ratio > damagedThreshold ? 1f : // 1단계는 페널티 없음
                ratio > severeThreshold ? damagedMultiplier :
                severeMultiplier;

            controller.SetSpeedMultiplier(multiplier);
        }
    }
}