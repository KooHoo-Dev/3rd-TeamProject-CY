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

        public void SetInputEnabled(bool value) => inputEnabled = value;
        public void SetSpeedMultiplier(float value) => speedMultiplier = Mathf.Clamp01(value);
        
        // HUD 표시용
        public VehicleInput LastInput { get; private set; }
        public float MaxSpeed => maxSpeed;
        public float SpeedRatio =>
            rb != null ? Mathf.Clamp01(rb.linearVelocity.magnitude / maxSpeed) : 0f;
        
        private IVehicleInputSource inputSource;
        public void SetInputSource(IVehicleInputSource source) => inputSource = source;
        
        private Vector2 lastPosition;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            lastPosition = transform.position;
        }
        
        // 비호스트(클라이언트)는 여기서 속도를 계산해야 부드럽게 이어집니다
        private void Update()
        {
            if (rb != null && rb.simulated == false)
            {
                float clientSpeed = ((Vector2)transform.position - lastPosition).magnitude / Time.deltaTime;
                CY.Vehicle.SoundManager.Instance?.UpdateVehicleSpeed(clientSpeed);
                lastPosition = transform.position;
            }
        }

        private void FixedUpdate()
        {
            if (inputSource == null) return;
            
            // 모두가 조작을 읽어옵니다.
            VehicleInput input = inputEnabled ? inputSource.Read() : default;
            LastInput = input;
            
            // 모두가 이벤트를 쏩니다 (미니게임 멈춤 현상 완벽 해결)
            VehicleSoundEvents.Raise(input);
            
            if (rb.simulated)
            {
                // 호스트는 기존처럼 물리 엔진의 정확한 속도를 사용합니다.
                CY.Vehicle.SoundManager.Instance?.UpdateVehicleSpeed(rb.linearVelocity.magnitude);
                lastPosition = transform.position; 
            }

            // 비호스트(클라이언트)는 물리 연산을 하지 않고 여기서 안전하게 퇴근
            if (rb.simulated == false) return; 
            
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

            rb.linearVelocity = forward * f + right * (s * (1f - sideFriction));
        }

        private void ClampSpeed(VehicleInput input)
        {
            float limit = input.CurrentGear == Gear.R ? reverseMaxSpeed : maxSpeed;
            if (rb.linearVelocity.magnitude > limit)
                rb.linearVelocity = rb.linearVelocity.normalized * limit;
        }
    }
}