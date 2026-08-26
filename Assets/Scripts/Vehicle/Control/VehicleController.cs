using UnityEngine;

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
    [SerializeField, Range(0f, 1f)] private float sideFriction = 0.9f;
        
    private Rigidbody2D rb;
    private IVehicleInputSource inputSource;
    
    private bool inputEnabled = true;
    private float speedMultiplier = 1f;
    
    public void SetInputEnabled(bool value) => inputEnabled = value;
    public void SetSpeedMultiplier(float value) => speedMultiplier = Mathf.Clamp01(value);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        inputSource = GetComponent<IVehicleInputSource>();

        if (inputSource == null)
            Debug.LogError($"{name}: IVehicleInputSource 구현체가 붙어있지 않습니다.", this);
    }
    
    private void FixedUpdate()
    {
        if (inputSource == null) return;

        VehicleInput input = inputEnabled ? inputSource.Read() : default;
        
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
