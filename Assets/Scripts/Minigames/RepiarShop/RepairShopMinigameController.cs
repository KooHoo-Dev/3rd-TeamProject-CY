using UnityEngine;

public class RepairShopMinigameController : MonoBehaviour
{
    public RepairShopMinigameView view;
    
    private int oldScrewsRemoved = 0;
    private int newScrewsFastened = 0;
    
    private bool isOldPlateDetached = false; // 나사 4개가 풀렸는지
    private bool isOldPlateDiscarded = false; // 기존 번호판을 버렸는지
    private bool isNewPlateAttached = false;  // 새 번호판을 제자리에 놨는지

    private void Start()
    {
        view.InitializeView();
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
    public bool CanDragOldPlate() => isOldPlateDetached && !isOldPlateDiscarded;
    public bool CanDragNewPlate() => isOldPlateDiscarded && !isNewPlateAttached;
    
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
