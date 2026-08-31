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

        [Header("미니게임 참조")] 
        [SerializeField] private GasStationMinigameController gasStationMinigameController;
        [SerializeField] private MinigameUI minigameUI;
        
        private NetworkSession session;
        private CheckpointZone activeCheckpoint;
        
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
            
            SendClientReadyAsync().Forget();
        }

        private void OnDisable()
        {
            CheckpointZone.MissionReady -= HandleCheckpointMission;
            if (session != null) session.MessageReceived -= HandleNetworkMessage;
            if (gasStationMinigameController != null) 
                gasStationMinigameController.MinigameEnded -= HandleFuelMiniGameEnded;
        }

        private async UniTaskVoid SendClientReadyAsync()
        {
            if (session == null || session.Me == null)
            {
                Debug.LogWarning("[미니게임] 서버 참여 정보가 없어 준비 완료 메시지를 보내지 못했습니다.");
                return;
            }

            await session.SendAsync(new MinigameClientReadyMessage());
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
            activeCheckpoint = checkpoint;
            RequestMiniGameStart(GetMiniGameType(checkpoint.MissionType));
        }

        private string GetMiniGameType(MissionType missionType)
        {
            return missionType switch
            {
                MissionType.GasStation => "fuel",
                MissionType.Barricade => "barricade",
                MissionType.LicensePlate => "license_plate",
                _ => null
            };
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

                    if (message == null) return;
                    
                    CurrentMiniGameType = message.MiniGameType;
                    MiniGameStartHandler(message);
                    minigameUI.SetCurrentMiniGameType(message.MiniGameType);
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
                    
                    if (Session.IsHost && activeCheckpoint != null)
                    {
                        activeCheckpoint.CompleteMission();
                        activeCheckpoint = null;
                    }
                    break;
                case "game_aborted":
                    gasStationMinigameController.HandleGameAborted();
                    break;
            }
        }

        private void MiniGameStartHandler(MiniGameStartedMessage message)
        {
            switch (message.MiniGameType)
            {
                case "fuel":
                    gasStationMinigameController.HandleMiniGameStarted(message);
                    return;
                case "barricade": return;
                case "license_plate": return;
            }
        }
        
        private void HandleFuelMiniGameEnded()
        {
            CurrentMiniGameType = null;
            minigameUI.HideMinigameUI();
        }
    }
}
