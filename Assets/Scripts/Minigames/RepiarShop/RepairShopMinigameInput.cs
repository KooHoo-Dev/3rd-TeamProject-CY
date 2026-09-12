using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CY.Minigame
{

    [RequireComponent(typeof(CanvasGroup))]
    public class RepairShopMinigameInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public RepairShopMinigameController controller;
        public bool isOldPlate;

        private RectTransform rectTransform;
        private Canvas canvas;
        private CanvasGroup canvasGroup;
        
        private const float DragSendInterval = 0.05f; // 초당 20회 전송
        private bool isDragging;
        private float nextDragSendTime;

        public void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            canvas = GetComponentInParent<Canvas>();
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Controller에 권한이 있는지 물어보고 없으면 무시
            if (isOldPlate && !controller.CanDragOldPlate()) return;
            if (!isOldPlate && !controller.CanDragNewPlate()) return;
            
            // 2. 번호판을 꽉 잡는 효과음
            CY.Vehicle.SoundManager.Instance?.PlayRepairShopPlateGrabSound();

            isDragging = true;
            canvasGroup.alpha = 0.8f;
            canvasGroup.blocksRaycasts = false; // 드롭 영역 인식을 위해 끄기
            rectTransform.DOKill(); // 진행 중인 애니메이션이 있다면 정지
            
            controller.SendPlateDrag(isOldPlate, rectTransform.anchoredPosition);
            nextDragSendTime = Time.unscaledTime + DragSendInterval;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
            
            if (Time.unscaledTime < nextDragSendTime) return; // 0.05초 간격으로 서버에 드래그 좌표 전송
            
            controller.SendPlateDrag(isOldPlate, rectTransform.anchoredPosition);
            nextDragSendTime = Time.unscaledTime + DragSendInterval;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging) return;
            
            // 3. 번호판을 떼거나 놓는 효과음
            CY.Vehicle.SoundManager.Instance?.PlayRepairShopPlateDropSound();

            isDragging = false;
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;

            controller.SendPlateDrag(isOldPlate, rectTransform.anchoredPosition); // 드롭 직전 마지막 위치 동기화

            GameObject droppedOn = eventData.pointerCurrentRaycast.gameObject;
            
            // 로컬에서는 영역만 판정해서 서버로 보냄
            bool isValidDrop = isOldPlate
                ? droppedOn != null && droppedOn.CompareTag("TrashZone")
                : droppedOn != null && droppedOn.CompareTag("DropZone"); 
            
            controller.HandlePlateDropped(isOldPlate, isValidDrop);
        }


    }
}
