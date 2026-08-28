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
    
    public TextMeshProUGUI resultText;
    
    private Vector2 oldPlateOrigin;
    private Vector2 newPlateOrigin;
    
    public void InitializeView()
    {
        oldPlateOrigin = oldPlateRect.anchoredPosition;
        newPlateOrigin = newPlateRect.anchoredPosition;

        resultText.gameObject.SetActive(false);

        foreach (var screw in oldScrews) screw.gameObject.SetActive(true);
        foreach (var screw in newScrews)
        {
            screw.gameObject.SetActive(false);
            screw.GetComponent<Image>().color = new Color(1, 1, 1, 0.3f); // 초기엔 반투명
        }
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
            newScrews[index].interactable = false;
        }
    }
    
    public void SnapBackPlate(bool isOldPlate)
    {
        RectTransform targetRect = isOldPlate ? oldPlateRect : newPlateRect;
        Vector2 originPos = isOldPlate ? oldPlateOrigin : newPlateOrigin;

        targetRect.DOKill();
        targetRect.DOAnchorPos(originPos, 0.3f).SetEase(Ease.OutBack);
    }
    
    public void DiscardOldPlate()
    {
        oldPlateRect.gameObject.SetActive(false);
    }
    
    public void AttachNewPlate(RectTransform targetDropZone)
    {
        newPlateRect.position = targetDropZone.position;
        
        // 부착 완료 후 새 나사 구멍 활성화
        foreach (var screw in newScrews)
        {
            screw.gameObject.SetActive(true);
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
