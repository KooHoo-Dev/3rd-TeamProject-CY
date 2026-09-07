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

        public string PositionId => positionId;

        private string currentCharacterId;
        private Vector2 homePosition;

        private void Awake()
        {
            homePosition = portraitImage.rectTransform.anchoredPosition;
            portraitImage.gameObject.SetActive(false);
        }

        public IEnumerator Show(string characterId, Sprite portrait)
        {
            if (currentCharacterId == characterId) yield break;
            if (currentCharacterId != null) yield return Hide();

            currentCharacterId = characterId;
            portraitImage.sprite = portrait;
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

            yield return portraitImage.rectTransform
                .DOAnchorPos(GetOutsidePosition(), slideDuration)
                .SetEase(Ease.InCubic)
                .WaitForCompletion();

            portraitImage.gameObject.SetActive(false);
            currentCharacterId = null;
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