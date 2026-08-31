using System;
using UnityEngine;
using  DG.Tweening;
using System.Collections;
using CY.Stages;

namespace CY.Minigame
{
    [DefaultExecutionOrder(-50)]
    public class MinigameUI : MonoBehaviour
    {
        public static MinigameUI Instance { get; private set; }
        
        [Header("Settings")] 
        [SerializeField] private RectTransform[] minigamePanel;
        [SerializeField] private GameObject[] explanationPanel;
        [SerializeField] private float slideDuration = 0.5f;
        
        public event Action OnGameStartRequested;

        private Vector2 hiddenPosition;
        private Vector2 visiblePosition;
        private RectTransform currentMiniGamePanel;
        private GameObject currentExplanationPanel;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            visiblePosition = Vector2.zero; 
            hiddenPosition = new Vector2(Screen.width, 0);
            
            currentMiniGamePanel = minigamePanel[0];
            currentExplanationPanel = explanationPanel[0];
            
            currentMiniGamePanel.anchoredPosition = hiddenPosition;
            if (explanationPanel != null) currentExplanationPanel.SetActive(false);
        }

        public void SetCurrentMiniGameType(string missionType)
        {
            switch (missionType)
            {
                case "fuel": 
                    UpdateCurrentPanels(0);
                    return;
                case "barricade": 
                    UpdateCurrentPanels(1);
                    return;
                case "license_plate": 
                    UpdateCurrentPanels(2);
                    return;
            }
        }

        private void UpdateCurrentPanels(int missionTypeIdx)
        {
            currentMiniGamePanel = minigamePanel[missionTypeIdx];
            currentExplanationPanel = explanationPanel[missionTypeIdx];
        }
        
        public void ShowMinigameUI()
        {
            currentMiniGamePanel.DOAnchorPos(visiblePosition, slideDuration).SetEase(Ease.OutQuad)
                .OnComplete(() => StartCoroutine(ShowExplanationRoutine())); 
        }
    
        private IEnumerator ShowExplanationRoutine()
        {
            yield return new WaitForSeconds(1.0f); 
        
            if (explanationPanel != null)
            {
                currentExplanationPanel.SetActive(true);
                CanvasGroup cg = currentExplanationPanel.GetComponent<CanvasGroup>();
                if (cg == null) cg = currentExplanationPanel.AddComponent<CanvasGroup>();
            
                cg.alpha = 0f;
                cg.DOFade(1f, 0.5f);
            }
        }
        
        public void OnGameStartButtonClicked()
        {
            if (explanationPanel != null) currentExplanationPanel.SetActive(false);
            
            OnGameStartRequested?.Invoke(); 
        }
    
        public void HideMinigameUI()
        {
            currentMiniGamePanel.DOAnchorPos(hiddenPosition, slideDuration).SetEase(Ease.InQuad);
        }
    }
}
