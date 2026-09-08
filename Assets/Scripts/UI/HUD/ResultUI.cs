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
    
    [SerializeField] private Button retryButton;
    
    [ContextMenu("테스트: 성공 화면 띄우기")]
    public void OnRoundSuccess()
    {
        resultPanel.SetActive(true);
        UpdateRetryButtonState();
        
        resultText.text = "성공!";
        resultText.color = Color.green;
        
        UpdateResultData(true);
    }
    
    [ContextMenu("테스트: 실패 화면 띄우기")]
    public void OnRoundFail()
    {
        resultPanel.SetActive(true);
        UpdateRetryButtonState();
        
        resultText.text = "실패...";
        resultText.color = Color.red;
        
        UpdateResultData(false);
    }

    private void UpdateRetryButtonState()
    {
        if (retryButton.interactable != null)
        {
            retryButton.interactable = Session.IsHost;
        }
    }

    private void UpdateResultData(bool isSuccess)
    {
        IRoundStatusProvider status = GetStatus();

        float remainingTime = status.RemainingTime;
        float durabilityRatio = status.DurabilityRatio;

        remainTimeText.text =
            $"남은 시간 : {remainingTime:0}초";

        remainDurabilityText.text =
            $"최종 내구도 : {durabilityRatio * vehicleDamage.MaxDurability:0}%";

        int starCount = 0;

        if (isSuccess)
        {
            float elapsedTime = roundTimer.Duration - remainingTime;

            if (elapsedTime <= 300f) 
            {
                starCount = 3;
            }
            else if (elapsedTime <= 420f) 
            {
                starCount = 2;
            }
            else if (elapsedTime <= 600f) 
            {
                starCount = 1;
            }
            else 
            {
                starCount = 0;
            }
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
        CY.Vehicle.SoundManager.Instance?.PlayUIClick();

        if (Session.IsHost == false) return;
        
        NetworkSession session = NetworkSession.Instance;
        if (session == null || session.IsConnected == false) return;
        
        session.RequestSceneChange(SceneManager.GetActiveScene().name);
    }

    public void OnQuitButtonClicked()
    {
        // 게임 종료(로비로 이동) 버튼 클릭음
        CY.Vehicle.SoundManager.Instance?.PlayUIClick();
        
        SceneManager.LoadScene("Lobby");
        
        SceneManager.LoadScene("Lobby");
    }
}
