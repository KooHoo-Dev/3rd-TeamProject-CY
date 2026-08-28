using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class RepairShopMinigameInput : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RepairShopMinigameController controller;
    public bool isOldPlate;
    
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    
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

        canvasGroup.alpha = 0.8f;
        canvasGroup.blocksRaycasts = false; // 드롭 영역 인식을 위해 끄기
        rectTransform.DOKill(); // 진행 중인 애니메이션이 있다면 즉시 정지
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isOldPlate && !controller.CanDragOldPlate()) return;
        if (!isOldPlate && !controller.CanDragNewPlate()) return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isOldPlate && !controller.CanDragOldPlate()) return;
        if (!isOldPlate && !controller.CanDragNewPlate()) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        GameObject droppedOn = eventData.pointerCurrentRaycast.gameObject;
        controller.HandlePlateDropped(isOldPlate, droppedOn);
    }
    
    
}
