using System;
using UnityEngine;
using  DG.Tweening;
using System.Collections;
using CY.Network;
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

        private Vector2 visiblePosition;
        private RectTransform currentMiniGamePanel;
        private GameObject currentExplanationPanel;
        
        private bool isOperator;
        public bool IsOperator { get; set; }
        
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            visiblePosition = Vector2.zero; 
            
            for (int i = 0; i < minigamePanel.Length; i++)
            {
                minigamePanel[i].anchoredPosition = GetHiddenPosition(minigamePanel[i]);
            }

            for (int i = 0; i < explanationPanel.Length; i++)
            {
                explanationPanel[i].SetActive(false);
            }
            
            currentMiniGamePanel = minigamePanel[0];
            currentExplanationPanel = explanationPanel[0];
        }

        private Vector2 GetHiddenPosition(RectTransform panel)
        {
            return new Vector2(panel.rect.width + 10f, 0f);
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
            
            OnGameStartRequested?.Invoke(); 
            
            if (explanationPanel != null && IsOperator)
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
        }
    
        public void HideMinigameUI()
        {
            currentMiniGamePanel.DOAnchorPos(GetHiddenPosition(currentMiniGamePanel), slideDuration).SetEase(Ease.InQuad);
        }
    }
}
