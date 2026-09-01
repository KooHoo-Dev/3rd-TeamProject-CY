using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace CY.Minigame
{

    public class RepairShopMinigameView : MonoBehaviour
    {
        [Header("UI")]
        public RectTransform oldPlateRect;
        public RectTransform newPlateRect;
        [SerializeField] private RectTransform plateSlot;
        
        public CanvasGroup oldPlateCanvasGroup;
        public CanvasGroup newPlateCanvasGroup;

        public Button[] oldScrews;
        public Button[] newScrews;

        public GameObject trashZone;
        public TextMeshProUGUI titleText; 
        public TextMeshProUGUI noticeText;
        public TextMeshProUGUI resultText;
        public TextMeshProUGUI assigneeText;

        private Vector2 oldPlateOrigin;
        private Vector2 newPlateOrigin;
        private bool currentIsOperator;
        
        private void Awake()
        {
            oldPlateOrigin = oldPlateRect.anchoredPosition;
            newPlateOrigin = newPlateRect.anchoredPosition;
        }
        
        public void Begin(bool isOperator)
        {
            ResetView();
            currentIsOperator = isOperator;
            
            // 처음엔 조작 불가, MinigameUI 신호 시 활성화
            SetOldScrewsInteractable(false);
            
            assigneeText.text = isOperator ? "Assignee : You" : "Assignee : OtherPlayer";
        }

        public void ResetView()
        {
            StopAllCoroutines();
            
            oldPlateRect.DOKill();
            newPlateRect.DOKill();
            oldPlateRect.anchoredPosition = oldPlateOrigin;
            newPlateRect.anchoredPosition = newPlateOrigin;
            
            oldPlateRect.gameObject.SetActive(true);
            newPlateRect.gameObject.SetActive(true);
            
            resultText.gameObject.SetActive(false);
            if (trashZone != null) trashZone.SetActive(false);

            foreach (var screw in oldScrews) screw.gameObject.SetActive(true);
            foreach (var screw in newScrews)
            {
                screw.gameObject.SetActive(false);
                screw.GetComponent<Image>().color = new Color(1, 1, 1, 0f);
            }
            
            SetTextAlpha(titleText, 0f);
            SetTextAlpha(noticeText, 0f);
            SetTextAlpha(assigneeText, 0f);
        }
        
        public void EnableInputForOperator()
        {
            if (currentIsOperator) SetOldScrewsInteractable(true);
        }
        
        private void SetOldScrewsInteractable(bool interactable)
        {
            foreach (var screw in oldScrews) screw.interactable = interactable;
        }
        
        public void FadeInGameTexts()
        {
            if (titleText != null) titleText.DOFade(1f, 1f); 
            if (noticeText != null) noticeText.DOFade(1f, 1f);
            if (assigneeText != null) assigneeText.DOFade(1f, 1f);
        }
        
        private void SetTextAlpha(TextMeshProUGUI txt, float alpha)
        {
            if (txt != null)
            {
                Color c = txt.color;
                c.a = alpha;
                txt.color = c;
            }
        }

        public void ApplyOldScrewRemoved(int index)
        {
            oldScrews[index].gameObject.SetActive(false);
        }

        public void ApplyNewScrewFastened(int index)
        {
            newScrews[index].GetComponent<Image>().color = Color.white;
            newScrews[index].interactable = false;
        }
        
        public void ApplyPlateDrag(bool isOldPlate, Vector2 anchoredPosition)
        {
            RectTransform plate = isOldPlate ? oldPlateRect : newPlateRect;
            plate.DOKill();
            plate.anchoredPosition = anchoredPosition;
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
            newPlateRect.anchoredPosition = plateSlot.anchoredPosition;

            foreach (var screw in newScrews)
            {
                screw.gameObject.SetActive(true);
                screw.interactable = currentIsOperator;
            }
        }

        public void ApplyCompleted()
        {
            SetOldScrewsInteractable(false);
            foreach (var screw in newScrews) screw.interactable = false;
            
            resultText.gameObject.SetActive(true);
            resultText.text = "미션 완료!";
            resultText.color = Color.green;
            resultText.transform.localScale = Vector3.zero;
            resultText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);
        }
        
        public void ApplyAborted()
        {
            SetOldScrewsInteractable(false);
            foreach (var screw in newScrews) screw.interactable = false;
            
            resultText.gameObject.SetActive(true);
            resultText.text = "Pause";
            resultText.color = Color.gray;
            resultText.transform.localScale = Vector3.zero;
            resultText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBounce);
        }




    }

}
