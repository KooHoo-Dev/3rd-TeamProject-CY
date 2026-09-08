using CY.Network;
using UnityEngine;
using UnityEngine.UI;

public class TutorialEndButton : MonoBehaviour
{
    [SerializeField] private Button tutorialEndButton;
    [SerializeField] private string nextSceneName = "PlayTest";

    private bool sceneChangeRequested;

    private void Awake()
    {
        if (tutorialEndButton == null)
            tutorialEndButton = GetComponent<Button>();
    }

    private void Start()
    {
        if (tutorialEndButton != null)
            tutorialEndButton.interactable = Session.IsHost;
    }

    public void TutorialEndButtonPressed()
    {
        if (Session.IsHost == false) return;
        if (sceneChangeRequested) return;

        NetworkSession session = NetworkSession.Instance;

        if (session == null || session.IsConnected == false) return;

        sceneChangeRequested = true;

        if (tutorialEndButton != null) tutorialEndButton.interactable = false;

        session.RequestSceneChange(nextSceneName);
    }
}