using CY.Network;
using CY.Vehicle;
using UnityEngine;
using UnityEngine.UI;

namespace CY.Vehicle
{
    public class TutorialFailureController : MonoBehaviour
    {
        [Header("대상")] [SerializeField] private VehicleDamage vehicleDamage;
        [SerializeField] private VehicleController vehicleController;
        [SerializeField] private NetworkRoundStatus networkStatus;

        [Header("UI")] [SerializeField] private Button retryButton;

        [Header("재시작 씬")] [SerializeField] private string tutorialSceneName = "Tutorial";

        private bool tutorialFailed;

        private void Awake()
        {
            if (retryButton != null)
                retryButton.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (vehicleDamage != null)
                vehicleDamage.Destroyed += HandleVehicleDestroyed;
        }

        private void OnDisable()
        {
            if (vehicleDamage != null)
                vehicleDamage.Destroyed -= HandleVehicleDestroyed;
        }

        private void Update()
        {
            if (tutorialFailed) return;

            // 비호스트는 RoundStateSync로 전달받는 내구도 상태를 기준으로 실패 처리.
            if (Session.IsHost == false
                && networkStatus != null
                && networkStatus.DurabilityRatio <= 0f)
            {
                EnterFailedState();
            }
        }

        private void HandleVehicleDestroyed()
        {
            // 실제 내구도 감소·파괴 판정은 호스트에서만 발생한다.
            if (Session.IsHost == false) return;

            EnterFailedState();
        }

        private void EnterFailedState()
        {
            if (tutorialFailed) return;

            tutorialFailed = true;

            // 호스트·비호스트 모두 로컬 차량 조작을 중지한다.
            if (vehicleController != null)
                vehicleController.SetInputEnabled(false);

            if (retryButton != null)
            {
                retryButton.gameObject.SetActive(true);

                // 기존 씬 전환 정책과 맞춰 호스트만 재시작 가능.
                retryButton.interactable = Session.IsHost;
            }
        }

        // RestartButton의 On Click()에 연결할 메서드
        public void OnRetryButtonClicked()
        {
            if (tutorialFailed == false) return;
            if (Session.IsHost == false) return;

            NetworkSession session = NetworkSession.Instance;

            if (session == null || session.IsConnected == false) return;

            if (retryButton != null)
                retryButton.interactable = false;

            // 서버가 scene_change를 전체에 브로드캐스트한다.
            // 모든 클라이언트가 Tutorial 씬을 새로 연다.
            session.RequestSceneChange(tutorialSceneName);
        }
    }
}