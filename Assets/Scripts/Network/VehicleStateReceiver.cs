using UnityEngine;

public struct VehicleSnapshot
{
    public Vector2 Position;
    public float   Rotation;
    public Vector2 Velocity;
}

public class VehicleStateReceiver : MonoBehaviour
{
    [SerializeField] private float positionSmoothTime  = 0.08f;
    [SerializeField] private float rotationSmoothSpeed = 720f;
    
    private Vector2 targetPosition;
    private float   targetRotation;
    private Vector2 smoothVelocity;
    private bool    hasSnapshot;

    public void ApplySnapshot(VehicleSnapshot snapshot)
    {
        targetPosition = snapshot.Position;
        targetRotation = snapshot.Rotation;
        hasSnapshot    = true;
    }
    
    private void Awake()
    {
        if (Session.IsHost)
        {
            enabled = false;      // 호스트는 물리로 직접 움직인다
            return;
        }
        
        // 비호스트는 물리를 끄고 받은 값으로만 움직인다
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
    }

    private void Update()
    {
        if (!hasSnapshot) return;
    }
}