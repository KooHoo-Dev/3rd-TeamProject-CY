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

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (inputSource == null) return;
            
            // 호스트와 비호스트 모두 무조건 조작 입력값을 읽어옵니다
            VehicleInput input = inputEnabled ? inputSource.Read() : default;
            
            // 이전 조작과 비교해서 '무언가 바뀌었을 때만' 이벤트를 쏩니다
            // 이렇게 하면 1초에 50번씩 드드드드 쏘는 기관총 현상이 완벽히 사라집니다
            bool isInputChanged = (input.CurrentGear != LastInput.CurrentGear) ||
                                  (input.Accel != LastInput.Accel) ||
                                  (input.Brake != LastInput.Brake);

            if (isInputChanged)
            {
                VehicleSoundEvents.Raise(input);
            }
            
            LastInput = input;
            
            // 사운드 매니저에게 현재 속도(magnitude)를 실시간으로 알려줍니다
            CY.Vehicle.SoundManager.Instance?.UpdateVehicleSpeed(rb.linearVelocity.magnitude);
            
            // 비호스트(클라이언트)는 실제 물리적인 이동 연산은 하지 않으므로 여기서 함수를 종료합니다
            if (rb.simulated == false) return; // 내 물리로 움직이는 상태가 아닐 경우 (비호스트)
            
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