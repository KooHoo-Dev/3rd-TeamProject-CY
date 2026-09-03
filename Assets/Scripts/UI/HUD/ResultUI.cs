using CY.Core;
using CY.Vehicle;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultUI : MonoBehaviour
{
    [Header("Settings")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText; 
    public TextMeshProUGUI remainTimeText; 
    public TextMeshProUGUI remainDurabilityText;
    
    public Image[] starImages; 
    public Sprite fullStarSprite;
    public Sprite emptyStarSprite;
    
    public RoundTimer roundTimer; 
    public VehicleDamage vehicleDamage;
    
    [ContextMenu("테스트: 성공 화면 띄우기")]
    public void OnRoundSuccess()
    {
        resultPanel.SetActive(true);
        resultText.text = "성공!";
        resultText.color = Color.green;
        
        UpdateResultData(true);
    }
    
    [ContextMenu("테스트: 실패 화면 띄우기")]
    public void OnRoundFail()
    {
        resultPanel.SetActive(true);
        resultText.text = "실패...";
        resultText.color = Color.red;
        
        UpdateResultData(false);
    }

    private void UpdateResultData(bool isSuccess)
    {
        float timePercent = 0f;
        float hpPercent = 0f;
        
        if (roundTimer != null)
        {
            remainTimeText.text = $"남은 시간 : {roundTimer.RemainingTime:0.#}초 / {roundTimer.Duration:0.#}초";
            
            if (roundTimer.Duration > 0)
                timePercent = (roundTimer.RemainingTime / roundTimer.Duration) * 100f;
        }

        if (vehicleDamage != null)
        {
            remainDurabilityText.text = $"최종 내구도 : {vehicleDamage.Durability:0.#}"; 
            hpPercent = (vehicleDamage.Durability / vehicleDamage.MaxDurability) * 100f; 
        }
        
        int starCount = 0;
        
        if (isSuccess)
        {
            if (timePercent >= 35f && hpPercent >= 65f) starCount = 3;
            else if (timePercent >= 15f && hpPercent >= 35f) starCount = 2;
            else starCount = 1;
        }
        
        for (int i = 0; i < starImages.Length; i++)
        {
            if (i < starCount) starImages[i].sprite = fullStarSprite;
            else starImages[i].sprite = emptyStarSprite;
        }
        
    }
    
    public void OnRetryButtonClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnQuitButtonClicked()
    {
        SceneManager.LoadScene("Lobby");
    }
    
    

}
