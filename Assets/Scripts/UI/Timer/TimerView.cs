using UnityEngine;
using TMPro;
using DG.Tweening;

public class TimerView : MonoBehaviour
{
    [Header("Settings")]
    public TextMeshProUGUI timerText;

    private bool isTensionActive = false;

    public void Initialize()
    {
        timerText.DOKill();
        timerText.color = Color.white;
        timerText.transform.localScale = Vector3.one;
        isTensionActive = false;
    }

    public void UpdateTimerText(float currentTime)
    {
        float displayTime = Mathf.Max(0, currentTime);
        
        int minutes = Mathf.FloorToInt(displayTime / 60F);
        int seconds = Mathf.FloorToInt(displayTime - minutes * 60);
        
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void ActivateTensionEffect()
    {
        if (isTensionActive) return;
        isTensionActive = true;

        timerText.color = Color.red;
        timerText.transform.DOScale(1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }
}
