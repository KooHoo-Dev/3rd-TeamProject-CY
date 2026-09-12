using UnityEngine;
using System;
using DG.Tweening;

public class TimerController : MonoBehaviour
{
    public TimerView view;

    private float timeLimit = 300f; 
    private float currentTime;
    private bool isTimerRunning = false;
    
    public event Action OnTimeOver; // 타임 오버시 호출

    private void Start()
    {
        StartTimer();
    }

    public void StartTimer()
    {
        currentTime = timeLimit;
        isTimerRunning = true;
        
        view.Initialize();
        view.UpdateTimerText(currentTime);
    }

    private void Update()
    {
        if (!isTimerRunning) return;

        currentTime -= Time.deltaTime;
        
        if (currentTime <= 10f && currentTime > 0f)
        {
            view.ActivateTensionEffect();
        }
        
        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isTimerRunning = false;
            
            view.UpdateTimerText(currentTime);
            
            // 결과 화면 컨트롤러에 신호
            OnTimeOver?.Invoke(); 
        }
        else
        {
            view.UpdateTimerText(currentTime);
        }
    }

    // 타이머를 강제로 멈출 때
    public void StopTimer()
    {
        isTimerRunning = false;
        view.timerText.DOKill(); 
    }

    // 결과 화면에 데이터 넘겨줄 때
    public float GetRemainTime()
    {
        return Mathf.Max(0, currentTime);
    }
}
