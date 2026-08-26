using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class MissionReadyEvent : UnityEvent<CheckpointZone> { }

[RequireComponent(typeof(BoxCollider2D))]
public class CheckpointZone: MonoBehaviour
{
    [Header("경유지 설정")]
    [SerializeField] private MissionType missionType;
    [SerializeField] private float stopSpeedThreshold = 0.05f;
    
    [Header("미션 시작 신호")]
    [SerializeField] private MissionReadyEvent onMissionReady;
    
    public MissionType MissionType => missionType;
    
    private BoxCollider2D zone;
    private Rigidbody2D vehicleBody;
    private IVehicleInputSource vehicleInput;
    private BoxCollider2D vehicleBox;
    private bool completed; // 이미 완료한 미션은 다시 진행 못함
    private VehicleController lockedVehicle;
    private RoundManager roundManager;

    private void Reset()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }
    
    private void Awake()
    {
        zone = GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (completed) return;
        
        Rigidbody2D body = other.attachedRigidbody;
        if (body == null) return;
        
        IVehicleInputSource input = body.GetComponent<IVehicleInputSource>();
        if (input == null) return;
        
        vehicleBody = body;
        vehicleInput = input;
        vehicleBox = body.GetComponent<BoxCollider2D>();
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.attachedRigidbody == vehicleBody)
        {
            vehicleBody = null;
            vehicleInput = null;
            vehicleBox = null;
        }
    }

    private void Update()
    {
        if (completed) return;
        if (vehicleBody == null) return;
        if (vehicleInput.Read().CurrentGear != Gear.N) return;
        if (vehicleBody.linearVelocity.magnitude > stopSpeedThreshold) return;
        if (!IsFullyInside()) return;
        
        completed = true;
        
        lockedVehicle = vehicleBody.GetComponent<VehicleController>();
        if (lockedVehicle != null) lockedVehicle.SetInputEnabled(false);
        
        Debug.Log($"[Checkpoint] {missionType} 미션 시작", this);
        onMissionReady.Invoke(this);
    }

    /// <summary>차의 네 모서리가 모두 구역 안에 있는지 검사한다.</summary>
    private bool IsFullyInside()
    {
        if (vehicleBox == null) return false;
        
        Vector2 offset = vehicleBox.offset; // 로컬좌표. 중심에서의 offset
        Vector2 halfSize = vehicleBox.size * 0.5f;
        
        if (!IsCornerInside(offset + new Vector2(halfSize.x, halfSize.y))) return false;
        if (!IsCornerInside(offset + new Vector2(halfSize.x, -halfSize.y))) return false;
        if (!IsCornerInside(offset + new Vector2(-halfSize.x, halfSize.y))) return false;
        if (!IsCornerInside(offset + new Vector2(-halfSize.x, -halfSize.y))) return false;
        
        return true;
    }

    private bool IsCornerInside(Vector2 localCorner)
    {
        Vector2 world = vehicleBox.transform.TransformPoint(localCorner);
        return zone.OverlapPoint(world);
    }
    
    [ContextMenu("미션 완료 (테스트)")]
    public void CompleteMission()
    {
        if (lockedVehicle != null)
        {
            lockedVehicle.SetInputEnabled(true);
            lockedVehicle = null;
        }

        Debug.Log($"[Checkpoint] {missionType} 미션 완료", this);
        roundManager?.OnCheckpointCleared(this);  
    }
    
    public void Bind(RoundManager manager) => roundManager = manager;
}