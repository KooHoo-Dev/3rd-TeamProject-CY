using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class RepairShopMinigameView : MonoBehaviour
{
    [Header("UI")]
    public RectTransform oldPlateRect;
    public RectTransform newPlateRect;
    public CanvasGroup oldPlateCanvasGroup;
    public CanvasGroup newPlateCanvasGroup;
    
    public Button[] oldScrews;
    public Button[] newScrews;
    
    public GameObject trashZone;
    public TextMeshProUGUI resultText;
    
    private Vector2 oldPlateOrigin;
    private Vector2 newPlateOrigin;
    
    public void InitializeView()
    {
        oldPlateOrigin = oldPlateRect.anchoredPosition;
        newPlateOrigin = newPlateRect.anchoredPosition;

        resultText.gameObject.SetActive(false);
        if (trashZone != null) trashZone.SetActive(false);

        foreach (var screw in oldScrews) screw.gameObject.SetActive(true);
        foreach (var screw in newScrews) screw.gameObject.SetActive(false);
    }
    
    public void ApplyOldScrewState(int index, bool isVisible)
    {
        oldScrews[index].gameObject.SetActive(isVisible);
    }
    
    public void ApplyNewScrewState(int index, bool isFastened)
    {
        if (isFastened)
        {
            newScrews[index].GetComponent<Image>().color = Color.white;
            newScrews[index].enabled = false;
        }
    }
    
    public void SnapBackPlate(bool isOldPlate)
    {
        RectTransform targetRect = isOldPlate ? oldPlateRect : newPlateRect;
        Vector2 originPos = isOldPlate ? oldPlateOrigin : newPlateOrigin;

        targetRect.DOKill();
        targetRect.DOAnchorPos(originPos, 0.3f).SetEase(Ease.OutBack);
    }
    
    public void ShowTrashZone()
    {
        if (trashZone != null)
        {
            trashZone.SetActive(true);
            
            CanvasGroup cg = trashZone.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.5f);
            }
        }
    }
    
    public void DiscardOldPlate()
    {
        oldPlateRect.gameObject.SetActive(false);
        if (trashZone != null) trashZone.SetActive(false);
    }
    
    public void AttachNewPlate(RectTransform targetDropZone)
    {
        newPlateRect.position = targetDropZone.position;
        
        foreach (var screw in newScrews)
        {
            screw.gameObject.SetActive(true);
            screw.GetComponent<Image>().color = new Color(1, 1, 1, 0f);
        }
    }
    
    public void ShowGameClear()
    {
        resultText.gameObject.SetActive(true);
        resultText.text = "미션 완료!";
        resultText.transform.localScale = Vector3.zero;
        resultText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);
    }
    
    
    
    
}
