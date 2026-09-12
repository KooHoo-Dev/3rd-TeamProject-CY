using DG.Tweening;
using UnityEngine;

namespace Intro
{
    public class IntroScreenShakeController : MonoBehaviour
    {
        [SerializeField] private float duration = 0.35f;
        [SerializeField] private Vector2 strength = new Vector2(25f, 18f);
        [SerializeField] private int vibrato = 15;

        private RectTransform target;
        private Vector2 homePosition;

        private void Awake()
        {
            target = transform as RectTransform;
            homePosition = target.anchoredPosition;
        }

        public void Shake()
        {
            target.DOKill();
            target.anchoredPosition = homePosition;

            target
                .DOShakeAnchorPos(duration, strength, vibrato)
                .SetUpdate(true)
                .OnComplete(() => target.anchoredPosition = homePosition);
        }
    }
}