using DG.Tweening;
using TMPro;
using UnityEngine;

namespace CY.UI
{
    public class TutorialGuideUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup guidePanel;
        [SerializeField] private TMP_Text guideText;
        [SerializeField] private float fadeDuration = 0.25f;
        [SerializeField] private float displayDuration = 3f;

        private Sequence sequence;

        private void Awake()
        {
            guidePanel.alpha = 0f;
            guidePanel.gameObject.SetActive(false);
        }

        public void Show(string message)
        {
            sequence?.Kill();

            guideText.text = message;
            guidePanel.gameObject.SetActive(true);
            guidePanel.alpha = 0f;

            sequence = DOTween.Sequence();
            sequence.Append(guidePanel.DOFade(1f, fadeDuration));
            sequence.AppendInterval(displayDuration);
            sequence.Append(guidePanel.DOFade(0f, fadeDuration));
            sequence.OnComplete(() => guidePanel.gameObject.SetActive(false));
        }
    }
}