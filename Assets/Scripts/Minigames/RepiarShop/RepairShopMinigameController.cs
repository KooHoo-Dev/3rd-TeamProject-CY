using UnityEngine;
using System.Collections;
using System;
using CY.Network; 
using Cysharp.Threading.Tasks;
using HelloServer;

namespace CY.Minigame
{
    public class RepairShopMinigameController : MonoBehaviour
    {
        private const string LicensePlateMiniGameType = "license_plate";
        private const float CompletedDisplaySeconds = 2f;
        
        public RepairShopMinigameView view;
        private NetworkSession session;
        private Coroutine completedRoutine;
        
        private bool IsOperator;
        private bool IsActive;
        private bool CanSendInput;

        private int oldScrewsRemoved;
        private bool isOldPlateDiscarded; // 기존 번호판을 버렸는지
        private bool isNewPlateAttached; // 새 번호판을 제자리에 놨는지

        public event Action MinigameEnded;
        
        private void Awake()
        {
            if (view == null) view = GetComponent<RepairShopMinigameView>();
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
            if (!gameObject.activeInHierarchy || !IsActive) return;

            MinigameUI.Instance.IsOperator = IsOperator;
            StartCoroutine(StartGameRoutine());
        }
        
        private IEnumerator StartGameRoutine()
        {
            yield return new WaitForSeconds(1f);
            view.FadeInGameTexts(); // DOTween 호출
            CanSendInput = IsOperator;  
            view.EnableInputForOperator();
        }
        
        private void ResetState()
        {
            IsOperator = false;
            IsActive = false;
            CanSendInput = false;

            oldScrewsRemoved = 0;
            isOldPlateDiscarded = false;
            isNewPlateAttached = false;

            if (view != null) view.ResetView();
        }

        public void RequestRemoveOldScrew(int screwIndex)
        {
            if (!IsActive || !IsOperator || !CanSendInput) return;

            session.SendAsync(new LicensePlateScrewMessage
            {
                Type = "license_plate_remove_old_screw",
                ScrewIndex = screwIndex
            }).Forget();
        }

        public void RequestFastenNewScrew(int screwIndex)
        {
            if (!IsActive || !IsOperator || !CanSendInput || !isNewPlateAttached) return;

            session.SendAsync(new LicensePlateScrewMessage
            {
                Type = "license_plate_fasten_new_screw",
                ScrewIndex = screwIndex
            }).Forget();
        }
        
        public void SendPlateDrag(bool isOldPlate, Vector2 anchoredPosition)
        {
            bool canDrag = isOldPlate ? CanDragOldPlate() : CanDragNewPlate();
            if (!canDrag) return;

            session.SendAsync(new LicensePlateDragMessage
            {
                IsOldPlate = isOldPlate,
                AnchoredX = anchoredPosition.x,
                AnchoredY = anchoredPosition.y
            }).Forget();
        }

        public void HandlePlateDropped(bool isOldPlate, bool isValidDrop)
        {
            bool canDrag = isOldPlate ? CanDragOldPlate() : CanDragNewPlate();
            if (!canDrag) return;

            session.SendAsync(new LicensePlateDropMessage
            {
                IsOldPlate = isOldPlate,
                IsValidDrop = isValidDrop
            }).Forget();
        }
        
        // 드래그 권한
        public bool CanDragOldPlate()
        {
            return IsActive && IsOperator && CanSendInput && oldScrewsRemoved == 4 && isOldPlateDiscarded == false;
        }

        public bool CanDragNewPlate()
        {
            return IsActive && IsOperator && CanSendInput && isOldPlateDiscarded && isNewPlateAttached == false;
        }
        
        public void HandleMiniGameStarted(MiniGameStartedMessage message)
        {
            if (message == null || message.MiniGameType != LicensePlateMiniGameType) return;

            ResetState();

            if (session == null || session.Me == null) return;

            IsActive = true;
            IsOperator = session.Me.Id == message.OperatorId;
            CanSendInput = false; // MinigameUI 신호 전 대기

            view.Begin(IsOperator);
        }
        
        public void HandlePlateDrag(LicensePlateDragMessage message)
        {
            if (!IsActive || IsOperator || message == null) return;

            view.ApplyPlateDrag(message.IsOldPlate, new Vector2(message.AnchoredX, message.AnchoredY));
        }
        
        public void HandlePlateSnapBack(LicensePlateDropResultMessage message)
        {
            if (!IsActive || message == null) return;
            view.SnapBackPlate(message.IsOldPlate);
        }
        
        public void HandleOldScrewRemoved(LicensePlateScrewMessage message)
        {
            if (!IsActive || message == null) return;

            oldScrewsRemoved++;
            view.ApplyOldScrewRemoved(message.ScrewIndex);

            if (oldScrewsRemoved == 4) view.ShowTrashZone();
        }

        public void HandleOldPlateDiscarded()
        {
            if (!IsActive) return;
            isOldPlateDiscarded = true;
            view.DiscardOldPlate();
        }

        public void HandleNewPlateAttached()
        {
            if (!IsActive) return;
            isNewPlateAttached = true;
            view.AttachNewPlate();
        }

        public void HandleNewScrewFastened(LicensePlateScrewMessage message)
        {
            if (!IsActive || message == null) return;
            view.ApplyNewScrewFastened(message.ScrewIndex);
        }

        public void HandleLicensePlateCompleted()
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

        private IEnumerator FinishAfterCompletedRoutine()
        {
            yield return new WaitForSeconds(CompletedDisplaySeconds);
            completedRoutine = null;
            ResetState();
            MinigameEnded?.Invoke();
        }

        private void StopCompletedRoutine()
        {
            if (completedRoutine == null) return;
            StopCoroutine(completedRoutine);
            completedRoutine = null;
        }
    }
    
}
