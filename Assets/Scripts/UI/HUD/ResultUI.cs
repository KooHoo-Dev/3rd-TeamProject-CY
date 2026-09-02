using CY.Core;
using CY.Vehicle;
using UnityEngine;
using TMPro;

public class ResultUI : MonoBehaviour
{
    [Header("Settings")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText; 
    public TextMeshProUGUI remainTimeText; 
    public TextMeshProUGUI remainDurabilityText;
    
    public RoundTimer roundTimer; 
    public VehicleDamage vehicleDamage;
    
    public void OnRoundSuccess()
    {
        resultPanel.SetActive(true);
        resultText.text = "성공!";
        resultText.color = Color.green;
        
        UpdateResultData();
    }
    
    public void OnRoundFail()
    {
        resultPanel.SetActive(true);
        resultText.text = "실패...";
        resultText.color = Color.red;
        
        UpdateResultData();
    }

    private void UpdateResultData()
    {
        if (roundTimer != null)
        {
            remainTimeText.text = $"남은 시간 : {roundTimer.RemainingTime:0.#}초";
        }

        if (vehicleDamage != null)
        {
            remainDurabilityText.text = $"남은 내구도 : {vehicleDamage.Durability:0.}"; 
        }
    }

}
