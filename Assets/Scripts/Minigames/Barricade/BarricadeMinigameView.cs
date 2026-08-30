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
        public GameObject explanationPanel;
        
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI assigneeText;
        public TextMeshProUGUI resultText;
        public TextMeshProUGUI noticeText;
        
        public Image overloadPanel;
        public Button[] fuseButtons;
        public Image[] rightCircuits;

        private Color fuseOffColor = Color.white;
        private Color fuseOnColor = Color.yellow;

        
        public void InitializeView()
        {
            explanationPanel.SetActive(false);
            overloadPanel.gameObject.SetActive(false);
            resultText.gameObject.SetActive(false);

            for (int i = 0; i < 3; i++)
            {
                fuseButtons[i].GetComponent<Image>().color = fuseOffColor;

                // 오른쪽 회로선 투명도를 0으로 
                Color c = rightCircuits[i].color;
                c.a = 0f;
                rightCircuits[i].color = c;
            }
            
            SetTextAlpha(titleText, 0f);
            SetTextAlpha(assigneeText, 0f);
            SetTextAlpha(noticeText, 0f);

        }
        
        public void ShowExplanationPanel()
        {
            if (explanationPanel != null)
            {
                explanationPanel.SetActive(true);
                CanvasGroup cg = explanationPanel.GetComponent<CanvasGroup>();
                if (cg == null) cg = explanationPanel.AddComponent<CanvasGroup>();
                
                cg.alpha = 0f;
                cg.DOFade(1f, 0.5f);
            }
        }
        
        public void HideExplanationPanel()
        {
            if (explanationPanel != null) explanationPanel.SetActive(false);
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
        
        public void SetFuseState(int index, bool isOn)
        {
            fuseButtons[index].GetComponent<Image>().color = isOn ? fuseOnColor : fuseOffColor;
            if (isOn)
            {
                rightCircuits[index].DOFade(1f, 0.3f);
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
