using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

namespace CY.Minigame
{

    public class BarricadeMinigameView : MonoBehaviour
    {
        [Header("UI")] 
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI assigneeText;
        public TextMeshProUGUI resultText;
        public TextMeshProUGUI noticeText;
        
        public Image overloadPanel;
        public Button[] fuseButtons;
        public Image[] rightCircuits;

        private Color fuseOffColor = Color.white;
        private Color fuseOnColor = Color.yellow;
        private bool currentIsOperator = false;

        public void Begin(bool isOperator)
        {
            ResetView();
            currentIsOperator = isOperator;
            
            // 처음에는 입력 불가 상태로 두고 Controller가 MinigameUI 신호를 받으면 풀어줌
            SetButtonsInteractable(false);
        }
        
        public void ResetView()
        {
            StopAllCoroutines();
            HideOverloadEffect();
            resultText.gameObject.SetActive(false);

            for (int i = 0; i < 3; i++)
            {
                fuseButtons[i].GetComponent<Image>().color = fuseOffColor;
                rightCircuits[i].DOKill();

                // 오른쪽 회로선 투명도를 0으로 
                Color c = rightCircuits[i].color;
                c.a = 0f;
                rightCircuits[i].color = c;
            }
            
            SetTextAlpha(titleText, 0f);
            SetTextAlpha(assigneeText, 0f);
            SetTextAlpha(noticeText, 0f);
            
            SetButtonsInteractable(false);

        }
        
        public void ApplyFuseProgress(int fuseIndex)
        {
            fuseButtons[fuseIndex].GetComponent<Image>().color = fuseOnColor;
            rightCircuits[fuseIndex].DOFade(1f, 0.3f);
        }
        
        public void ApplyAttemptFailed()
        {
            StartCoroutine(OverloadRoutine());
        }
        
        public void ApplyAborted()
        {
            SetButtonsInteractable(false);
            ShowResultText("Pause", Color.gray);
        }
        
        private IEnumerator OverloadRoutine()
        {
            SetButtonsInteractable(false); // 과부하 중 버튼 잠금
            ShowOverloadEffect();

            yield return new WaitForSeconds(2.0f);

            HideOverloadEffect();
            
            // 과부하가 끝나면 담당자만 버튼 다시 활성화
            SetButtonsInteractable(currentIsOperator); 
        }
        
        public void EnableInputForOperator()
        {
            if (currentIsOperator) SetButtonsInteractable(true);
        }

        private void SetButtonsInteractable(bool interactable)
        {
            foreach (var btn in fuseButtons)
            {
                btn.interactable = interactable;
            }
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
        
        public void ShowOverloadEffect()
        {
            for (int i = 0; i < 3; i++)
            {
                fuseButtons[i].GetComponent<Image>().color = fuseOffColor;
                rightCircuits[i].DOKill();
                rightCircuits[i].DOFade(0f, 0.1f);
            }

            overloadPanel.gameObject.SetActive(true);
            overloadPanel.color = new Color(1, 0, 0, 0.5f);
            overloadPanel.DOKill();
            overloadPanel.DOFade(0f, 0.2f).SetLoops(4, LoopType.Yoyo);
        }
        
        public void HideOverloadEffect()
        {
            overloadPanel.DOKill();
            overloadPanel.gameObject.SetActive(false);
        }
        

        public void ShowResultText(string message, Color color)
        {
            resultText.gameObject.SetActive(true);
            resultText.text = message;
            resultText.color = color;
            resultText.transform.localScale = Vector3.zero;
            resultText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
        }

    }

}
