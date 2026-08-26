using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class GasStationMinigame : MonoBehaviour
{
    [Header("UI")]
    public Image gaugeFill;          
    public Image fuelNozzle;         
    
    [Header("Settings")]
    public float fillSpeed = 0.1f;   // 1초에 오르는 양
    
    private bool isPressing = false; 
    private bool isFailed = false;
    
    private Color originalGaugeColor = Color.green;

    private void Start()
    {
        gaugeFill.fillAmount = 0f;
        gaugeFill.color = originalGaugeColor;
    }

    private void Update()
    {
        if (isFailed) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPressing = true;
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            isPressing = false;
            
            CheckResult();
        }

        if (isPressing)
        {
            gaugeFill.fillAmount += fillSpeed * Time.deltaTime;

            if (gaugeFill.fillAmount >= 1.0f)
            {
                isPressing = false;
                StartCoroutine(FailRoutine());
            }
        }
        
    }

    private void CheckResult()
    {
        if (gaugeFill.fillAmount >= 0.9f && gaugeFill.fillAmount <= 1.0f)
        {
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
        
        yield return new WaitForSeconds(0.5f);
        
        gaugeFill.fillAmount = 0f;
        gaugeFill.color = originalGaugeColor;
        
        isFailed = false;
        
        
    }
}
