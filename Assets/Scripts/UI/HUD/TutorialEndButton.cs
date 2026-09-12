using CY.Network;
using CY.Stages;
using UnityEngine;
using UnityEngine.UI;

public class TutorialEndButton : MonoBehaviour
{
    [SerializeField] private Button tutorialEndButton;
    [SerializeField] private string nextSceneName = "PlayTest";

    [Header("Tutorial End Condition")]
    [SerializeField] private TutorialStopCheckpoint stopCheckpoint;
    [SerializeField] private NetworkRoundStatus networkStatus;
    
    private bool sceneChangeRequested;

    private void Awake()
    {
        if (tutorialEndButton == null)
            tutorialEndButton = GetComponent<Button>();

        if (stopCheckpoint == null)
            stopCheckpoint = FindAnyObjectByType<TutorialStopCheckpoint>();

        if (networkStatus == null)
            networkStatus = FindAnyObjectByType<NetworkRoundStatus>();
    }

    private void Start()
    {
        RefreshButton();
    }

    private void Update()
    {
        RefreshButton();
    }
    
    private bool IsNetworkReady()
    {
        return NetworkSession.IsInitialized
               && NetworkSession.Instance != null
               && NetworkSession.Instance.IsConnected;
    }

    private void RefreshButton()
    {
        if (tutorialEndButton == null) return;

        bool tutorialCompleted = Session.IsHost
            ? stopCheckpoint != null && stopCheckpoint.IsCompleted
            : networkStatus != null && networkStatus.TutorialStopCompleted;

        tutorialEndButton.interactable =
            tutorialCompleted
            && Session.IsHost
            && IsNetworkReady()
            && sceneChangeRequested == false;
    }

    public void TutorialEndButtonPressed()
    {
        if (Session.IsHost == false)
        {
            Debug.LogWarning("[Tutorial] 호스트만 PlayTest로 이동할 수 있습니다.", this);
            return;
        }

        if (sceneChangeRequested) return;

        if (stopCheckpoint == null || stopCheckpoint.IsCompleted == false)
        {
            Debug.LogWarning("[Tutorial] 정차 체크포인트를 아직 완료하지 않았습니다.", this);
            return;
        }

        if (IsNetworkReady() == false)
        {
            Debug.LogWarning(
                "[Tutorial] 서버 연결이 없습니다. Lobby에서 방에 접속한 뒤 Tutorial로 진입해야 합니다.",
                this);
            return;
        }

        sceneChangeRequested = true;
        tutorialEndButton.interactable = false;

        Debug.Log("[Tutorial] PlayTest 씬 전환 요청 전송", this);

        NetworkSession.Instance.RequestSceneChange(nextSceneName);
    }
}