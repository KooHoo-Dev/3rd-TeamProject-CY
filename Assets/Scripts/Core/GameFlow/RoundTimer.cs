using UnityEngine;
using UnityEngine.Events;

public class RoundTimer: MonoBehaviour
{
    [Header("제한 시간")]
    [SerializeField] private float roundDuration = 180f;
    
    [Header("시간 소진 신호")]
    [SerializeField] private UnityEvent onTimeExpired;

    public float Duration      => roundDuration;
    public float RemainingTime => remainingTime;
    public float ElapsedTime   => elapsedTime;
    public bool  IsRunning     => isRunning;
    
    private float remainingTime;
    private float elapsedTime; // 경유지 미션으로 추가시간 받을 경우 계산해서 얻으면 음수가 나올 수 있으므로 따로 선언
    private bool  isRunning;
    
    public void StartRound() => StartRound(roundDuration);

    public void StartRound(float duration)
    {
        roundDuration = duration;
        remainingTime = duration;
        elapsedTime   = 0f;
        isRunning     = true;
    }
    
    public void StopRound()
    {
        isRunning = false;
    }

    public void AddTime(float seconds)
    {
        if (!isRunning) return;
        
        remainingTime += seconds;
        Debug.Log($"[Timer] +{seconds:0.#}초 → 남은 시간 {remainingTime:0.#}", this);
    }

    private void Update()
    {
        if (!Session.IsHost) return;
        if (!isRunning) return;
        
        remainingTime -= Time.deltaTime;
        elapsedTime   += Time.deltaTime;

        if (remainingTime > 0f) return;
        
        remainingTime = 0f;
        isRunning = false;
        
        Debug.Log("[Timer] 제한 시간 소진 — 라운드 실패", this);
        onTimeExpired.Invoke();
    }

    /// <summary>주유 미션 성공 시 보너스 시간을 더한다.</summary>
    [ContextMenu("보너스 +30초 (테스트)")]
    public void AddBonusTimeForTest() => AddTime(30f);
}