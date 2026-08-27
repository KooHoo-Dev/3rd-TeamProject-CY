using UnityEngine;
using  DG.Tweening;

public class MinigameUI : MonoBehaviour
{
    [Header("Settings")]
    public RectTransform minigamePanel; 
    public float slideDuration = 0.5f; 
    
    private Vector2 hiddenPosition; 
    private Vector2 visiblePosition; 

    private void Start()
    {
        visiblePosition = Vector2.zero; // 중앙 좌표
        
        hiddenPosition = new Vector2(Screen.width, 0);
        
        minigamePanel.anchoredPosition = hiddenPosition;
    }
    
    public void ShowMinigameUI()
    {
        minigamePanel.DOAnchorPos(visiblePosition, slideDuration).SetEase(Ease.OutQuad);
    }
    
    public void HideMinigameUI()
    {
        minigamePanel.DOAnchorPos(hiddenPosition, slideDuration).SetEase(Ease.InQuad);
    }
}
