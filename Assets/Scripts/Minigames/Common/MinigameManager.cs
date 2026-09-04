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
        [SerializeField] private BarricadeMinigameController barricadeMinigameController;
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
                gasStationMinigameController.MinigameEnded += HandleMiniGameEnded;
            if (barricadeMinigameController != null)
                barricadeMinigameController.MinigameEnded += HandleMiniGameEnded;
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
                gasStationMinigameController.MinigameEnded -= HandleMiniGameEnded;
            if (barricadeMinigameController != null) 
                barricadeMinigameController.MinigameEnded -= HandleMiniGameEnded;
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
            if (gasStationMinigameController == null
                && barricadeMinigameController == null) return;

            switch (type)
            {
                case "minigame_started":
                {
                    MiniGameStartedMessage message =
                        JsonConvert.DeserializeObject<MiniGameStartedMessage>(json);

                    if (message == null) return;
                    
                    CurrentMiniGameType = message.MiniGameType;
                    HandleMiniGameStarted(message);
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
                case "barricade_progress":
                    barricadeMinigameController.HandleBarricadeProgress(
                        JsonConvert.DeserializeObject<BarricadeProgressMessage>(json));
                    break;
                case "barricade_attempt_failed":
                    barricadeMinigameController.HandleBarricadeAttemptFailed();
                    break;
                case "barricade_completed":
                    barricadeMinigameController.HandleBarricadeCompleted();
                    
                    if (Session.IsHost && activeCheckpoint != null)
                    {
                        activeCheckpoint.CompleteMission();
                        activeCheckpoint = null;
                    }
                    break;
                case "game_aborted":
                    HandleGameAborted();
                    break;
            }
        }

        private void HandleMiniGameStarted(MiniGameStartedMessage message)
        {
            switch (message.MiniGameType)
            {
                case "fuel":
                    gasStationMinigameController.HandleMiniGameStarted(message);
                    return;
                case "barricade": 
                    barricadeMinigameController.HandleMiniGameStarted(message);
                    return;
                case "license_plate": return;
            }
        }

        private void HandleGameAborted()
        {
            switch (CurrentMiniGameType) 
            {
                case "fuel":
                    gasStationMinigameController.HandleGameAborted();
                    break;
                case "barricade":
                    barricadeMinigameController.HandleGameAborted();
                    break;
                case "license_plate": return;
            }
        }
        
        private void HandleMiniGameEnded()
        {
            CurrentMiniGameType = null;
            minigameUI.HideMinigameUI();
        }
    }
}
