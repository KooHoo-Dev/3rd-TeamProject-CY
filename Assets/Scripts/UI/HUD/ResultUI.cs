using CY.Core;
using CY.Network;
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
    public HostRoundStatus hostStatus;
    public NetworkRoundStatus networkStatus;
    
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
        IRoundStatusProvider status = GetStatus();

        float remainingTime = status.RemainingTime;
        float durabilityRatio = status.DurabilityRatio;

        remainTimeText.text =
            $"남은 시간 : {remainingTime:0.#}초 / {roundTimer.Duration:0.#}초";

        remainDurabilityText.text =
            $"최종 내구도 : {durabilityRatio * vehicleDamage.MaxDurability:0.#}";

        float timePercent = remainingTime / roundTimer.Duration * 100f;
        float hpPercent = durabilityRatio * 100f;

        int starCount = 0;

        if (isSuccess)
        {
            if (timePercent >= 35f && hpPercent >= 65f) starCount = 3;
            else if (timePercent >= 15f && hpPercent >= 35f) starCount = 2;
            else starCount = 1;
        }

        for (int i = 0; i < starImages.Length; i++)
        {
            starImages[i].sprite =
                i < starCount ? fullStarSprite : emptyStarSprite;
        }
    }

    private IRoundStatusProvider GetStatus()
    {
        return Session.IsHost ? hostStatus : networkStatus;
    }
    
    public void OnRetryButtonClicked()
    {
        // 다시 하기 버튼 클릭음
        CY.Vehicle.SoundManager.Instance?.PlayUIClick();
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnQuitButtonClicked()
    {
        // 게임 종료(로비로 이동) 버튼 클릭음
        CY.Vehicle.SoundManager.Instance?.PlayUIClick();
        
        SceneManager.LoadScene("Lobby");
        
        SceneManager.LoadScene("Lobby");
    }
    
    

}
