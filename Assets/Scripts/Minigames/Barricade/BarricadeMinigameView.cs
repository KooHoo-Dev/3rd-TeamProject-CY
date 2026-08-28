using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class BarricadeMinigameView : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI assigneeText;
    public TextMeshProUGUI resultText;
    public Image overloadPanel;
    public Button[] fuseButtons;
    public Image[] rightCircuits;
    
    private List<int> correctSequence = new List<int>(); // 이번 판의 정답 순서
    private int currentStep = 0; // 현재 맞춰야 할 순서 인덱스 (0~2)
    private bool isOverloaded = false; // 과부하(쿨타임) 상태인지 확인
    
    private Color fuseOffColor = Color.white; 
    private Color fuseOnColor = Color.yellow;

    private void Start()
    {
        overloadPanel.gameObject.SetActive(false);
        resultText.transform.localScale = Vector3.zero;
        
        InitializeGame();
    }

    public void InitializeGame()
    {
        currentStep = 0;
        isOverloaded = false;
        overloadPanel.gameObject.SetActive(false);
        
        for (int i = 0; i < 3; i++)
        {
            fuseButtons[i].GetComponent<Image>().color = fuseOffColor;
            
            // 오른쪽 회로선 투명도를 0으로 
            Color c = rightCircuits[i].color;
            c.a = 0f;
            rightCircuits[i].color = c;
        }
        
        correctSequence.Clear();
        correctSequence.Add(0);
        correctSequence.Add(1);
        correctSequence.Add(2);
        
        for (int i = 0; i < correctSequence.Count; i++) //(Fisher-Yates Shuffle 알고리즘)
        {
            int temp = correctSequence[i];
            int randomIndex = Random.Range(i, correctSequence.Count);
            correctSequence[i] = correctSequence[randomIndex];
            correctSequence[randomIndex] = temp;
        }
        
        // 정답 테스트용
        Debug.Log($"정답 순서: {correctSequence[0]} -> {correctSequence[1]} -> {correctSequence[2]}");
        
    }
    
    public void OnFuseClicked(int fuseIndex)
    {
        if (isOverloaded || currentStep >= 3) return;

        if (fuseIndex == correctSequence[currentStep])
        {
            fuseButtons[fuseIndex].GetComponent<Image>().color = fuseOnColor;
            
            rightCircuits[fuseIndex].DOFade(1f, 0.3f);
            
            currentStep++; // 성공
                
            if (currentStep >= 3)
            {
                ShowResultText("미션 성공!", Color.green);
            }
        }
        else
        {
            StartCoroutine(OverloadRoutine());
        }
            
    }

    private IEnumerator OverloadRoutine()
    {
        isOverloaded = true;
        
        for (int i = 0; i < 3; i++)
        {
            fuseButtons[i].GetComponent<Image>().color = fuseOffColor;
            
            rightCircuits[i].DOKill(); // 혹시 진행 중인 애니메이션이 있다면 캔슬
            rightCircuits[i].DOFade(0f, 0.1f); 
        }
            
        overloadPanel.gameObject.SetActive(true);
        overloadPanel.color = new Color(1, 0, 0, 0.5f);
            
        overloadPanel.DOKill();
        overloadPanel.DOFade(0f, 0.2f).SetLoops(4, LoopType.Yoyo);
            
        yield return new WaitForSeconds(2.0f);
            
        overloadPanel.gameObject.SetActive(false);
        currentStep = 0;
            
        isOverloaded = false;
            
    }
        
    private void ShowResultText(string message, Color color)
    {
        resultText.text = message;
        resultText.color = color;
        resultText.transform.localScale = Vector3.zero;
        resultText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
    }
    
}
