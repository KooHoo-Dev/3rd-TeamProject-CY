using System;
using UnityEngine;
using  DG.Tweening;
using System.Collections;

namespace CY.Minigame
{
    [DefaultExecutionOrder(-50)]
    public class MinigameUI : MonoBehaviour
    {
        public static MinigameUI Instance { get; private set; }
        
        [Header("Settings")] 
        public RectTransform minigamePanel;
        public float slideDuration = 0.5f;
        public GameObject explanationPanel;
        
        public event Action OnGameStartRequested;

        private Vector2 hiddenPosition;
        private Vector2 visiblePosition;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            visiblePosition = Vector2.zero; 
            hiddenPosition = new Vector2(Screen.width, 0);
        
            minigamePanel.anchoredPosition = hiddenPosition;
            if (explanationPanel != null) explanationPanel.SetActive(false);
        }
        
        public void ShowMinigameUI()
        {
            minigamePanel.DOAnchorPos(visiblePosition, slideDuration).SetEase(Ease.OutQuad)
                .OnComplete(() => StartCoroutine(ShowExplanationRoutine())); // 💡 슬라이드 끝나면 설명창 루틴 시작
        }
    
        private IEnumerator ShowExplanationRoutine()
        {
            yield return new WaitForSeconds(1.0f); 
        
            if (explanationPanel != null)
            {
                explanationPanel.SetActive(true);
                CanvasGroup cg = explanationPanel.GetComponent<CanvasGroup>();
                if (cg == null) cg = explanationPanel.AddComponent<CanvasGroup>();
            
                cg.alpha = 0f;
                cg.DOFade(1f, 0.5f);
            }
        }
        
        public void OnGameStartButtonClicked()
        {
            if (explanationPanel != null) explanationPanel.SetActive(false);
            
            OnGameStartRequested?.Invoke(); 
        }
    
        public void HideMinigameUI()
        {
            minigamePanel.DOAnchorPos(hiddenPosition, slideDuration).SetEase(Ease.InQuad);
        }
    }
}
