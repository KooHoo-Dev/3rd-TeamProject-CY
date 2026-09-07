using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Intro
{
    public enum SlideFrom
    {
        Left,
        Right,
        Bottom
    }

    public class PortraitSlot : MonoBehaviour
    {
        [SerializeField] private string positionId;
        [SerializeField] private Image portraitImage;
        [SerializeField] private SlideFrom slideFrom;
        [SerializeField] private float slideDistance = 1200f;
        [SerializeField] private float slideDuration = 0.35f;

        [Header("화자 강조")] 
        [SerializeField] private float speakingScale = 1.08f;
        [SerializeField, Range(0, 1)] private float inactiveBrightness = 0.8f;
        [SerializeField] private float emphasisDuration = 0.15f;
        
        public string PositionId => positionId;
        public bool IsVisible => currentCharacterId != null;

        private string currentCharacterId;
        private Vector2 homePosition;
        private Vector3 normalScale;
        private Color  normalColor;

        private void Awake()
        {
            homePosition = portraitImage.rectTransform.anchoredPosition;
            normalScale = portraitImage.rectTransform.localScale;
            normalColor = portraitImage.color;
            
            portraitImage.gameObject.SetActive(false);
        }

        public IEnumerator Show(string characterId, Sprite portrait)
        {
            if (currentCharacterId == characterId) yield break;
            if (currentCharacterId != null) yield return Hide();

            currentCharacterId = characterId;
            portraitImage.sprite = portrait;

            ResetVisualImmediately();
            
            portraitImage.rectTransform.anchoredPosition = GetOutsidePosition();
            portraitImage.gameObject.SetActive(true);

            yield return portraitImage.rectTransform
                .DOAnchorPos(homePosition, slideDuration)
                .SetEase(Ease.OutCubic)
                .WaitForCompletion();
        }

        public IEnumerator Hide()
        {
            if (currentCharacterId == null) yield break;

            ResetVisualImmediately();
            
            yield return portraitImage.rectTransform
                .DOAnchorPos(GetOutsidePosition(), slideDuration)
                .SetEase(Ease.InCubic)
                .WaitForCompletion();

            portraitImage.gameObject.SetActive(false);
            currentCharacterId = null;
        }

        public void SetSpeaking(bool isSpeaking)
        {
            if (IsVisible == false) return;

            Color targetColor = isSpeaking
                ? normalColor
                : new Color(
                    normalColor.r * inactiveBrightness,
                    normalColor.g * inactiveBrightness,
                    normalColor.b * inactiveBrightness,
                    normalColor.a);

            Vector3 targetScale = normalScale * (isSpeaking ? speakingScale : 1f);

            portraitImage.DOKill();
            portraitImage.rectTransform.DOKill();
            
            portraitImage
                .DOColor(targetColor, emphasisDuration)
                .SetEase(Ease.OutCubic);

            portraitImage.rectTransform
                .DOScale(targetScale, emphasisDuration)
                .SetEase(Ease.OutCubic);
        }
        
        private void ResetVisualImmediately()
        {
            portraitImage.DOKill();
            portraitImage.rectTransform.DOKill();

            portraitImage.color = normalColor;
            portraitImage.rectTransform.localScale = normalScale;
        }
        
        private Vector2 GetOutsidePosition()
        {
            return slideFrom switch
            {
                SlideFrom.Left => homePosition + Vector2.left * slideDistance,
                SlideFrom.Right => homePosition + Vector2.right * slideDistance,
                SlideFrom.Bottom => homePosition + Vector2.down * slideDistance
            };
        }
    }
}