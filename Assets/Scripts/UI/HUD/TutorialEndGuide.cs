using CY.Network;
using CY.Stages;
using UnityEngine;

namespace CY.UI
{
    public class TutorialStopGuidePresenter : MonoBehaviour
    {
        [TextArea]
        [SerializeField] private string guideMessage = "";

        private TutorialStopCheckpoint stopCheckpoint;
        private NetworkRoundStatus networkStatus;
        private TutorialGuideUI guideUI;
        private bool hasShown;

        private void Awake()
        {
            stopCheckpoint = FindAnyObjectByType<TutorialStopCheckpoint>();
            networkStatus = FindAnyObjectByType<NetworkRoundStatus>();
            guideUI = FindAnyObjectByType<TutorialGuideUI>();
        }

        private void Update()
        {
            if (hasShown)
                return;

            bool tutorialCompleted = Session.IsHost
                ? stopCheckpoint.IsCompleted
                : networkStatus.TutorialStopCompleted;

            if (tutorialCompleted == false)
                return;

            hasShown = true;
            guideUI.Show(guideMessage);
        }
    }
}