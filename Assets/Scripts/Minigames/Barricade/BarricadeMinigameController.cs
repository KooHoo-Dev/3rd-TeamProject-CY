using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CY.Network;

public class BarricadeFuseClickMessage
{
    public string Type { get; set; } = "barricade_fuse_click";
    public int FuseIndex { get; set; }
}

public class BarricadeProgressMessage
{
    public string Type { get; set; } = "barricade_progress";
    public int FuseIndex { get; set; }
}

namespace CY.Minigame
{

    public class BarricadeMinigameController : MonoBehaviour
    {
        private const string BarricadeMiniGameType = "barricade";
        private const float CompletedDisplaySeconds = 2f;
        
        public BarricadeMinigameView view;
        private NetworkSession session;
        private Coroutine completedRoutine;
        
        private bool IsOperator;
        private bool IsActive;
        private bool CanSendInput;

        public event Action MinigameEnded;
        
        private void Awake()
        {
            if (view == null) view = GetComponent<BarricadeMinigameView>();
            session = NetworkSession.Instance;
        }
        
        private void Start()
        {
            ResetState();
        }
        
        private void OnEnable()
        {
            if (MinigameUI.Instance != null)
            {
                MinigameUI.Instance.OnGameStartRequested += HandleGameStartSignal;
            }
        }

        private void OnDisable()
        {
            if (MinigameUI.Instance != null)
            {
                MinigameUI.Instance.OnGameStartRequested -= HandleGameStartSignal;
            }
        }

        private void HandleGameStartSignal()
        {
            if (gameObject.activeInHierarchy == false || IsActive == false) return;
            StartCoroutine(StartGameRoutine());
        }
        
        private IEnumerator StartGameRoutine()
        {
            yield return new WaitForSeconds(1.0f);
            view.FadeInGameTexts(); 
            CanSendInput = IsOperator; 
            view.EnableInputForOperator();   
        }
        
        private void ResetState()
        {
            IsActive = false;
            IsOperator = false;
            CanSendInput = false;

            if (view != null) view.ResetView();
        }
        
        public void SendFuseClick(int fuseIndex)
        {
            if (!IsActive || !IsOperator || !CanSendInput) return;

            if (fuseIndex < 0 || fuseIndex > 2) return;

            session.SendAsync(new BarricadeFuseClickMessage
            {
                FuseIndex = fuseIndex
            }).Forget();[cite: 15]
        }
        
        public void HandleMiniGameStarted(MiniGameStartedMessage message)
        {
            if (message == null || message.MiniGameType != BarricadeMiniGameType) return;

            ResetState();

            if (session == null || session.Me == null) return;

            IsActive = true;
            IsOperator = session.Me.Id == message.OperatorId;
            CanSendInput = false; // MinigameUI 신호 전까지 대기

            view.Begin(IsOperator);
        }
        
        private IEnumerator OverloadRoutine()
        {
            isOverloaded = true;
            view.ShowOverloadEffect();

            yield return new WaitForSeconds(2.0f);

            view.HideOverloadEffect();
            
            currentStep = 0;
            isOverloaded = false;
        }
    }

}
