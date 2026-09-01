using UnityEngine;
using System.Collections;
using System;
using CY.Network; 
using Cysharp.Threading.Tasks;

public class LicensePlateScrewMessage { public string Type { get; set; } public int ScrewIndex { get; set; } }
public class LicensePlateDragMessage { public string Type { get; set; } = "license_plate_drag"; public bool IsOldPlate { get; set; } public float AnchoredX { get; set; } public float AnchoredY { get; set; } }
public class LicensePlateDropMessage { public string Type { get; set; } = "license_plate_drop"; public bool IsOldPlate { get; set; } public bool IsValidDrop { get; set; } }
public class LicensePlateDropResultMessage { public string Type { get; set; } = "license_plate_plate_snap_back"; public bool IsOldPlate { get; set; } }

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

        private int oldScrewsRemoved = 0;
        private int newScrewsFastened = 0;

        private bool isGameStarted = false;
        private bool isOldPlateDetached = false; // 나사 4개가 풀렸는지
        private bool isOldPlateDiscarded = false; // 기존 번호판을 버렸는지
        private bool isNewPlateAttached = false; // 새 번호판을 제자리에 놨는지

        private void Start()
        {
            view.InitializeView();
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
            if (!gameObject.activeInHierarchy) return;
            StartCoroutine(StartGameRoutine());
        }
        
        private IEnumerator StartGameRoutine()
        {
            yield return new WaitForSeconds(1f);
            view.FadeInGameTexts(); // DOTween 호출
            isGameStarted = true;  
        }

        public void RequestRemoveOldScrew(int index)
        {
            if (isOldPlateDetached) return;

            oldScrewsRemoved++;
            view.ApplyOldScrewState(index, false);

            if (oldScrewsRemoved >= 4)
            {
                isOldPlateDetached = true;

                view.ShowTrashZone();
            }
        }

        public void RequestFastenNewScrew(int index)
        {
            if (!isNewPlateAttached || newScrewsFastened >= 4) return;

            newScrewsFastened++;
            view.ApplyNewScrewState(index, true);

            if (newScrewsFastened >= 4)
            {
                view.ShowGameClear();
            }
        }

        // 드래그 권한
        public bool CanDragOldPlate() => isGameStarted && isOldPlateDetached && isOldPlateDiscarded == false;
        public bool CanDragNewPlate() => isGameStarted && isOldPlateDiscarded && isNewPlateAttached == false;

        public void HandlePlateDropped(bool isOldPlate, GameObject droppedOnZone)
        {
            if (isOldPlate)
            {
                if (droppedOnZone != null && droppedOnZone.CompareTag("TrashZone"))
                {
                    isOldPlateDiscarded = true;
                    view.DiscardOldPlate();
                }
                else
                {
                    view.SnapBackPlate(true); // 오답 위치 시 제자리 복귀
                }
            }
            else
            {
                if (droppedOnZone != null && droppedOnZone.CompareTag("DropZone"))
                {
                    isNewPlateAttached = true;
                    view.AttachNewPlate(droppedOnZone.GetComponent<RectTransform>());
                }
                else
                {
                    view.SnapBackPlate(false);
                }
            }
        }
    }
}
