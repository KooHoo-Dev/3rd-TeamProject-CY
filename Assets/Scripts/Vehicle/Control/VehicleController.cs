using UnityEngine;

namespace CY.Vehicle
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class VehicleController : MonoBehaviour
    {
        [Header("가속")] 
        [SerializeField] private float enginePower = 20f;
        [SerializeField] private float maxSpeed = 12f;
        [SerializeField] private float reverseMaxSpeed = 5f;

        [Header("브레이크")] 
        [SerializeField] private float brakePower = 30f;

        [Header("회전")] 
        [SerializeField] private float turnRate = 180f;
        [SerializeField] private float turnSpeedThreshold = 4f;

        [Header("사이드저항")] 
        [SerializeField, Range(0f, 1f)]
        private float sideFriction = 0.9f;

        private Rigidbody2D rb;

        private bool inputEnabled = true;
        private float speedMultiplier = 1f;
        
        private float slipGrip = 1f;
        private float slipUntil;

        public void SetInputEnabled(bool value) => inputEnabled = value;
        public void SetSpeedMultiplier(float value) => speedMultiplier = Mathf.Clamp01(value);
        
        // HUD 표시용
        public VehicleInput LastInput { get; private set; }
        public float MaxSpeed => maxSpeed;
        public float SpeedRatio =>
            rb != null ? Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeed) : 0f;
        
        private IVehicleInputSource inputSource;
        public void SetInputSource(IVehicleInputSource source) => inputSource = source;
        
        /// <summary>잠시 옆으로 미끄러지기 쉽게 만든다. ObstacleAI 에게 밀려날 때 빠져나갈 수 있다.</summary>
        public void ApplySlip(float grip, float duration)
        {
            slipGrip = Mathf.Clamp01(grip);
            slipUntil = Mathf.Max(slipUntil, Time.time + duration); // 연달아 불렸을 때 나중 것이 앞의 시간을 잘라먹지 않게 하기 위해 Max로
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (inputSource == null) return;
            if (rb.simulated == false) return; // 내 물리로 움직이는 상태가 아닐 경우 (비호스트)
            
            VehicleInput input = inputEnabled ? inputSource.Read() : default;
            LastInput = input;
            
            VehicleSoundEvents.Raise(input);
            
            ApplyEngine(input);
            ApplyBrake(input);
            ApplySteering(input);
            KillSideVelocity();
            ClampSpeed(input);
        }
        

        private void ApplyEngine(VehicleInput input)
        {
            if (input.CurrentGear == Gear.N) return;
            if (input.Brake > 0f) return;

            float dir = input.CurrentGear == Gear.D ? 1f : -1f;
            rb.AddForce(transform.up * (input.Accel * enginePower * dir));
        }

        private void ApplyBrake(VehicleInput input)
        {
            if (input.Brake <= 0f) return;

            rb.linearVelocity = Vector2.MoveTowards(
                rb.linearVelocity,
                Vector2.zero,
                brakePower * input.Brake * Time.fixedDeltaTime
            );
        }

        private void ApplySteering(VehicleInput input)
        {
            // 부호 있는 전방 속도 : 후진 중이면 음수 → 좌/우회전이 반대로 걸린다
            float forwardSpeed = Vector2.Dot(rb.linearVelocity, transform.up);
            float factor = Mathf.Clamp(forwardSpeed / turnSpeedThreshold, -1f, 1f);

            // 2D는 각도 증가가 반시계(좌회전)라 부호를 뒤집는다
            float delta = -input.Steer * turnRate * factor * Time.fixedDeltaTime;
            rb.MoveRotation(rb.rotation + delta);
        }

        private void KillSideVelocity()
        {
            Vector2 forward = transform.up;
            Vector2 right = transform.right;

            float f = Vector2.Dot(rb.linearVelocity, forward);
            float s = Vector2.Dot(rb.linearVelocity, right);
            
            // 밀려나는 중이면 접지력을 낮춰서 옆으로 흐를 수 있게 한다
            float scale = Time.time < slipUntil ? slipGrip : 1f;
            float friction = sideFriction * scale;

            rb.linearVelocity = forward * f + right * (s * (1f - friction));
        }

        private void ClampSpeed(VehicleInput input)
        {
            float limit = input.CurrentGear == Gear.R ? reverseMaxSpeed : maxSpeed;
            if (rb.linearVelocity.magnitude > limit)
                rb.linearVelocity = rb.linearVelocity.normalized * limit;
        }
    }
}