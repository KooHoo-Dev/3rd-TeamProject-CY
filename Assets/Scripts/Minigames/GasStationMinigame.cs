using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;
using TMPro;

public class GasStationMinigame : MonoBehaviour
{
    [Header("UI")]
    public Image gaugeFill;          
    public Image fuelNozzle;  
    
    public TextMeshProUGUI titleText;       
    public TextMeshProUGUI assigneeText;    
    public TextMeshProUGUI resultText;
    
    [Header("Settings")]
    public float fillSpeed = 0.1f;   // 1초에 오르는 양
    public float nozzleMoveDistance = 50f;
    public float nozzleMoveDuration = 0.2f;
    
    private bool isPressing = false; 
    private bool isFailed = false;
    
    private Color originalGaugeColor = Color.green;
    
    private Vector2 nozzleIdlePos;
    private Vector2 nozzleActivePos;

    private void Start()
    {
        gaugeFill.fillAmount = 0f;
        gaugeFill.color = originalGaugeColor;
        
        nozzleIdlePos = fuelNozzle.rectTransform.anchoredPosition;
        nozzleActivePos = nozzleIdlePos + new Vector2(nozzleMoveDistance, 0f);
        
        resultText.text = "";
        resultText.transform.localScale = Vector3.zero;
    }

    private void Update()
    {
        if (isFailed) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPressing = true;
            fuelNozzle.rectTransform.DOKill();
            fuelNozzle.rectTransform.DOAnchorPos(nozzleActivePos, nozzleMoveDuration).SetEase(Ease.OutQuad);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isPressing = false;
            RetractNozzle();
            
            CheckResult();
        }

        if (isPressing)
        {
            gaugeFill.fillAmount += fillSpeed * Time.deltaTime;

            if (gaugeFill.fillAmount >= 1.0f)
            {
                isPressing = false;
                RetractNozzle();;
                StartCoroutine(FailRoutine());
            }
        }
        
    }

    
    private void RetractNozzle()
    {
        fuelNozzle.rectTransform.DOKill();
        fuelNozzle.rectTransform.DOAnchorPos(nozzleIdlePos, nozzleMoveDuration).SetEase(Ease.OutQuad);
    }
    
    private void CheckResult()
    {
        if (gaugeFill.fillAmount >= 0.9f && gaugeFill.fillAmount <= 1.0f)
        {
            ShowResultText("Success!", Color.green);
            Debug.Log("주유 성공! 미션 클리어!");
        }
        else
        {
            StartCoroutine(FailRoutine());
        }
    }

    private IEnumerator FailRoutine()
    {
        isFailed = true;
        gaugeFill.color = Color.red;
        
        ShowResultText("Fail!", Color.red);
        
        yield return new WaitForSeconds(0.5f);
        
        gaugeFill.fillAmount = 0f;
        gaugeFill.color = originalGaugeColor;
        
        resultText.transform.DOScale(Vector3.zero, 0.2f);
        
        isFailed = false;
        
        
    }

    private void ShowResultText(string message, Color color)
    {
        resultText.text = message;
        resultText.color = color;
        
        resultText.transform.localScale = Vector3.zero;
        resultText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.OutBack);
    }
    
    
}
