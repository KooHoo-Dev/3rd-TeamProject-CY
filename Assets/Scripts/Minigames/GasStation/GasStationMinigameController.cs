using UnityEngine;
using System;
using CY.Network;
using CY.Stages;
using Cysharp.Threading.Tasks;
using HelloServer;
using System.Collections;

namespace CY.Minigame
{
    [RequireComponent(typeof(GasStationMinigameView))]
    public class GasStationMinigameController : MonoBehaviour
    {
        private const string FuelMiniGameType = "fuel";
        private const float CompletedDisplaySeconds = 2f;
        
        private GasStationMinigameView view;
        private NetworkSession session;
        private Coroutine completedRoutine;

        private bool IsOperator;
        private bool IsActive;
        private bool CanSendInput;

        public event Action MinigameEnded;

        private string myUserId;

        private void Awake()
        {
            view = GetComponent<GasStationMinigameView>();
            session = NetworkSession.Instance;
        }

        private void Start()
        {
            ResetState();
        }

        public void SendFuelPress()
        {
            if (!IsActive || !IsOperator || !CanSendInput) return;

            session.SendAsync(new TypeOnly { Type = "fuel_press" }).Forget();
        }

        public void SendFuelRelease()
        {
            if (!IsActive || !IsOperator || !CanSendInput) return;

            session.SendAsync(new TypeOnly { Type = "fuel_release" }).Forget();
        }

        public void HandleMiniGameStarted(MiniGameStartedMessage message)
        {
            if (message == null || message.MiniGameType != FuelMiniGameType) return;
            
            ResetState();

            if (session == null || session.Me == null)
            {
                Debug.LogWarning("[미니게임] 내 사용자 ID가 없어 담당자를 판별할 수 없습니다.");
                return;
            }
            
            IsActive = true;
            IsOperator = session.Me.Id == message.OperatorId;
            CanSendInput = false;

            view.Begin(IsOperator);
            
            StartCoroutine(ShowExplanationRoutine());
        }
        
        private IEnumerator ShowExplanationRoutine()
        {
            yield return new WaitForSeconds(1.5f);
            view.ShowExplanationPanel();
        }
        
        public void OnGameStartButtonClicked()
        {
            view.HideExplanationPanel();
            StartCoroutine(StartGameRoutine());
        }
        
        private IEnumerator StartGameRoutine()
        {
            yield return new WaitForSeconds(1.0f);
            view.FadeInGameTexts();
            
            CanSendInput = IsOperator; 
        }

        public void HandleFuelState(FuelStateMessage message)
        {
            if (IsActive == false || message == null) return;

            view.ApplyFuelState(message.GaugePercent, message.IsPressed);
        }

        public void HandleFuelAttemptFailed()
        {
            if (!IsActive) return;

            CanSendInput = IsOperator;
            view.ApplyAttemptFailed();
        }

        public void HandleFuelCompleted()
        {
            if (!IsActive || completedRoutine != null) return;

            CanSendInput = false;
            view.ApplyCompleted();

            completedRoutine = StartCoroutine(FinishAfterCompletedRoutine());
        }

        public void HandleGameAborted()
        {
            if (!IsActive) return;

            StopCompletedRoutine();
            
            CanSendInput = false;
            IsActive = false;
            IsOperator = false;
            
            view.ApplyAborted();
            MinigameEnded?.Invoke();
        }

        private System.Collections.IEnumerator FinishAfterCompletedRoutine()
        {
            yield return new WaitForSeconds(CompletedDisplaySeconds);

            completedRoutine = null;
            ResetState();
            MinigameEnded?.Invoke();
        }
        
        private void ResetState()
        {
            IsActive = false;
            IsOperator = false;
            CanSendInput = false;

            if (view != null) view.ResetView();
        }
        
        private void StopCompletedRoutine()
        {
            if (completedRoutine == null)
                return;

            StopCoroutine(completedRoutine);
            completedRoutine = null;
        }
    }
}

