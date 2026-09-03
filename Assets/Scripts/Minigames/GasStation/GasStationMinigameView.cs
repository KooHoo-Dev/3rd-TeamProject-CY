using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;

namespace CY.Minigame
{
    public class GasStationMinigameView : MonoBehaviour
    {
        [Header("UI")] 
        public Image gaugeFill;
        public Image fuelNozzle;

        public TextMeshProUGUI titleText;
        public TextMeshProUGUI assigneeText;
        public TextMeshProUGUI noticeText;
        public TextMeshProUGUI resultText;
        public RectTransform fuelButtonRect;

        [Header("Settings")] 
        public float fillSpeed = 0.1f; // 1초에 오르는 양
        public float nozzleMoveDistance = 50f;
        public float nozzleMoveDuration = 0.2f;

        private Color originalGaugeColor = Color.green;

        private Vector2 nozzleIdlePos;
        private Vector2 nozzleActivePos;

        private bool isCurrentlyPressed = false;

        private void Awake()
        {
            nozzleIdlePos = fuelNozzle.rectTransform.anchoredPosition;
            nozzleActivePos = nozzleIdlePos + new Vector2(nozzleMoveDistance, 0f);
        }

        public void Begin(bool isOperator)
        {
            ResetView();

            fuelButtonRect.gameObject.SetActive(isOperator);
            assigneeText.text = isOperator
                ? "<담당자>"
                : "<관전자>";
        }

        public void FadeInGameTexts()
        {
            if (titleText != null) titleText.DOFade(1f, 1f);
            if (assigneeText != null) assigneeText.DOFade(1f, 1f);
            if (noticeText != null) noticeText.DOFade(1f, 1f);
        }

        private void SetTextAlpha(TextMeshProUGUI txt, float alpha)
        {
            if (txt != null)
            {
                Color c = txt.color;
                c.a = alpha;
                txt.color = c;
            }
        }

        public void ApplyFuelState(float gaugePercent, bool isPressed)
        {
            // 서버에서 오는 게이지 퍼센트(0~100)를 fillAmount(0~1) 비율로 변환
            gaugeFill.fillAmount = Mathf.Clamp01(gaugePercent / 100f);
            
            if (isPressed && isCurrentlyPressed == false)
            {
                isCurrentlyPressed = true;
                
                fuelNozzle.rectTransform.DOKill();
                fuelNozzle.rectTransform.DOAnchorPos(nozzleActivePos, nozzleMoveDuration).SetEase(Ease.OutQuad);

                fuelButtonRect.DOKill();
                fuelButtonRect.DOScale(Vector3.one * 0.9f, 0.1f);
            }
            else if (isPressed == false && isCurrentlyPressed)
            {
                isCurrentlyPressed = false;
                RetractNozzleButton();
            }
        }

        public void ApplyAttemptFailed()
        {
            StartCoroutine(FailRoutine());
        }

        public void ApplyCompleted()
        {
            RetractNozzleButton();
            ShowResultText("Success!", Color.green);
        }

        public void ApplyAborted()
        {
            RetractNozzleButton();
            ShowResultText("Pause", Color.gray);
        }

        public void ResetView()
        {
            StopAllCoroutines();
            
            gaugeFill.color = originalGaugeColor;
            gaugeFill.fillAmount = 0f;
            
            resultText.transform.localScale = Vector3.zero;
            isCurrentlyPressed = false;
            
            SetTextAlpha(titleText, 0f);
            SetTextAlpha(assigneeText, 0f);
            SetTextAlpha(noticeText, 0f);
            
            RetractNozzleButton();
        }


        private void RetractNozzleButton()
        {
            fuelNozzle.rectTransform.DOKill();
            fuelNozzle.rectTransform.DOAnchorPos(nozzleIdlePos, nozzleMoveDuration).SetEase(Ease.OutQuad);

            fuelButtonRect.DOKill();
            fuelButtonRect.DOScale(Vector3.one, 0.1f);
        }


        private IEnumerator FailRoutine()
        {
            gaugeFill.color = Color.red;

            ShowResultText("Fail!", Color.red);
            
            yield return new WaitForSeconds(0.5f);

            gaugeFill.fillAmount = 0f;
            gaugeFill.color = originalGaugeColor;

            resultText.transform.DOKill();
            resultText.transform.DOScale(Vector3.zero, 0.2f);

        }

        private void ShowResultText(string message, Color color)
        {
            resultText.text = message;
            resultText.color = color;

            resultText.transform.DOKill();
            resultText.transform.localScale = Vector3.zero;
            resultText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }


    }
}
