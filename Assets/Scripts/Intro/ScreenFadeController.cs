using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Intro
{
    public class ScreenFadeController : MonoBehaviour
    {
        [SerializeField] private Image fadeImage;
        
        private void Awake()
        {
            fadeImage.raycastTarget = false;
            fadeImage.color = Color.black;
        }
        
        public IEnumerator FadeIn(float duration)
        {
            yield return fadeImage
                .DOFade(0f, duration)
                .SetEase(Ease.InOutSine)
                .WaitForCompletion();
        }
        
        public IEnumerator FadeOut(Color color, float duration)
        {
            color.a = 0f;
            fadeImage.color = color;

            yield return fadeImage
                .DOFade(1f, duration)
                .SetUpdate(true)
                .WaitForCompletion();
        }
    }
}