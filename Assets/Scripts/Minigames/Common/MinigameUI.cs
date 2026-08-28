using System;
using UnityEngine;
using  DG.Tweening;

namespace CY.Minigame
{
    public class MinigameUI : MonoBehaviour
    {
        [Header("Settings")] 
        public RectTransform minigamePanel;
        public float slideDuration = 0.5f;

        private Vector2 hiddenPosition;
        private Vector2 visiblePosition;

        private void Awake()
        {
            visiblePosition = minigamePanel.anchoredPosition;
            hiddenPosition = visiblePosition + Vector2.right * minigamePanel.rect.width;

            minigamePanel.anchoredPosition = hiddenPosition;
        }

        public void ShowMinigameUI()
        {
            minigamePanel.DOKill();
            minigamePanel.DOAnchorPos(visiblePosition, slideDuration).SetEase(Ease.OutQuad);
        }

        public void HideMinigameUI()
        {
            minigamePanel.DOKill();
            minigamePanel.DOAnchorPos(hiddenPosition, slideDuration).SetEase(Ease.InQuad);
        }
    }
}
