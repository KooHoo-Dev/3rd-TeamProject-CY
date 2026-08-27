using UnityEngine;
using UnityEngine.Events;

public class RoundManager : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private RoundTimer timer;
    [SerializeField] private VehicleController vehicle;
    [SerializeField] private Transform startPoint;
    [SerializeField] private CheckpointZone[] checkpoints;
    
    [Header("결과 신호")]
    [SerializeField] private UnityEvent onRoundSucceeded;
    [SerializeField] private UnityEvent onRoundFailed;
    
    public int  ClearedCount { get; private set; }
    public int  TotalCheckpoints => checkpoints.Length;
    public bool IsFinished => finished;
    
    private bool finished;
    private VehicleDamage damage;
    
    private void Start()
    {
        if (!Session.IsHost) { enabled = false; return; }
        
        foreach (CheckpointZone zone in checkpoints)
            zone.Bind(this);
        
        damage = vehicle.GetComponent<VehicleDamage>();
        damage?.Bind(this);

        StartRound();
    }
    
    public void StartRound()
    {
        finished = false;
        ClearedCount = 0;

        MoveVehicleToStart();
        damage?.ResetDurability();
        vehicle.SetInputEnabled(true);
        timer.StartRound();

        Debug.Log("[Round] 라운드 시작", this);
    }

    private void MoveVehicleToStart()
    {
        Rigidbody2D rb = vehicle.GetComponent<Rigidbody2D>();
        rb.linearVelocity  = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.position = startPoint.position;
        rb.rotation = startPoint.eulerAngles.z;
    }

    /// <summary>CheckpointZone 이 미션 완료 시 호출한다.</summary>
    public void OnCheckpointCleared(CheckpointZone zone)
    {
        if (finished) return;
        
        ClearedCount++;
        Debug.Log($"[Round] 경유지 {ClearedCount}/{TotalCheckpoints} 완료 ({zone.MissionType})", this);
    }

    /// <summary>DestinationZone 이 차량 진입 시 호출한다.</summary>
    public void OnDestinationReached()
    {
        if (finished) return;
        
        if (ClearedCount < TotalCheckpoints)
        {
            Debug.Log($"[Round] 경유지 미완료 {ClearedCount}/{TotalCheckpoints} — 도착 무효", this);
            return;
        }
        
        Finish(true);
    }
    
    /// <summary>제한 시간 소진, 내구도 0 등 실패 원인에서 호출한다.</summary>
    public void FailRound()
    {
        if (finished) return;
        Finish(false);
    }

    private void Finish(bool success)
    {
        finished = true;
        
        timer.StopRound();
        vehicle.SetInputEnabled(false);
        
        if (success)
        {
            onRoundSucceeded.Invoke();
            Debug.Log($"[Round] 성공! 남은 시간 {timer.RemainingTime:0.#}초 / 걸린 시간 {timer.ElapsedTime:0.#}초", this);
        }
        else
        {
            onRoundFailed.Invoke();
            Debug.Log($"[Round] 실패. 경유지 {ClearedCount}/{TotalCheckpoints}", this);
        }
    }
}