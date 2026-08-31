using System;
using CY.Network;
using Cysharp.Threading.Tasks;
using HelloServer;
using Newtonsoft.Json;
using CY.Stages;
using UnityEngine;

namespace CY.Minigame
{
    public class MinigameManager : MonoBehaviour
    {
        // 임시용 필드, 실제 동작은 CheckPointZone.cs의 이벤트로부터 수신
        private const string FuelMiniGameType = "fuel";

        [Header("미니게임 참조")] 
        [SerializeField] private GasStationMinigameController gasStationMinigameController;
        [SerializeField] private MinigameUI minigameUI;

        [Header("MiniGameTest 임시 시작 설정")]
        // 실제 CheckpointZone 연동 전까지 주유 미니게임을 자동 요청한다.
        [SerializeField] private bool requestTemporaryMiniGameOnStart = true;
        [SerializeField] private string temporaryMiniGameType = FuelMiniGameType;
        
        private NetworkSession session;
        
        public string CurrentMiniGameType { get; private set; }
        
        public void Awake()
        {
            session = NetworkSession.Instance;
        }

        private void OnEnable()
        {
            CheckpointZone.MissionReady += HandleCheckpointMission;
            if (session == null) session = NetworkSession.Instance;
            if (session != null) session.MessageReceived += HandleNetworkMessage;
            if (gasStationMinigameController != null)
                gasStationMinigameController.MinigameEnded += HandleFuelMiniGameEnded;
        }

        private void Start()
        {
            Debug.Log(
                $"[미니게임] 진입: session={session != null}, " +
                $"me={session?.Me?.Id ?? "null"}, " +
                $"connected={session?.IsConnected}");

            
            SendReadyAndRequestTemporaryMiniGameAsync().Forget();
        }

        private void OnDisable()
        {
            CheckpointZone.MissionReady -= HandleCheckpointMission;
            if (session != null) session.MessageReceived -= HandleNetworkMessage;
            if (gasStationMinigameController != null) 
                gasStationMinigameController.MinigameEnded -= HandleFuelMiniGameEnded;
        }

        private async UniTaskVoid SendReadyAndRequestTemporaryMiniGameAsync()
        {
            if (session == null || session.Me == null)
            {
                Debug.LogWarning("[미니게임] 서버 참여 정보가 없어 준비 완료 메시지를 보내지 못했습니다.");
                return;
            }

            await session.SendAsync(new MinigameClientReadyMessage());

            if (!requestTemporaryMiniGameOnStart) return;

            RequestMiniGameStart(temporaryMiniGameType);
        }

        // 추후 CheckPointZone 이벤트가 호출할 공용 시작 요청 이벤트
        public void RequestMiniGameStart(string miniGameType)
        {
            if (session == null || session.Me == null) return;
            if (string.IsNullOrEmpty(miniGameType)) return;
            
            session.SendAsync(new MinigameStartRequestMessage
            {
                MiniGameType = miniGameType.Trim().ToLowerInvariant()
            }).Forget();
        }

        private void HandleCheckpointMission(CheckpointZone checkpoint)
        {
            if (checkpoint.MissionType == MissionType.GasStation)
                RequestMiniGameStart(FuelMiniGameType);
        }

        private void HandleNetworkMessage(string type, string json)
        {
            if (gasStationMinigameController == null) return;

            switch (type)
            {
                case "minigame_started":
                {
                    MiniGameStartedMessage message =
                        JsonConvert.DeserializeObject<MiniGameStartedMessage>(json);

                    if (message == null || message.MiniGameType != FuelMiniGameType) return;

                    CurrentMiniGameType = FuelMiniGameType;
                    
                    gasStationMinigameController.HandleMiniGameStarted(message);
                    minigameUI.ShowMinigameUI();
                    break;
                }
                case "fuel_state":
                    gasStationMinigameController.HandleFuelState(
                        JsonConvert.DeserializeObject<FuelStateMessage>(json));
                    break;
                case "fuel_attempt_failed":
                    gasStationMinigameController.HandleFuelAttemptFailed();
                    break;
                case "fuel_completed":
                    gasStationMinigameController.HandleFuelCompleted();
                    break;
                case "game_aborted":
                    gasStationMinigameController.HandleGameAborted();
                    break;
            }
        }

        private void HandleFuelMiniGameEnded()
        {
            CurrentMiniGameType = null;
            minigameUI.HideMinigameUI();
        }
    }
}
