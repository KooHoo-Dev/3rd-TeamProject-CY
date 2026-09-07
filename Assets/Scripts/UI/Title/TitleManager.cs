using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class TitleManager : MonoBehaviour
{
    [Header("Setting")]
    [SerializeField] private RectTransform titleRect;        
    [SerializeField] private CanvasGroup titleCanvasGroup;   // 제목 투명도 조절용
    [SerializeField] private CanvasGroup pressAnyKeyGroup;  
    [SerializeField] private CanvasGroup fadePanel;  
    [SerializeField] private RectTransform shakeContainer;
    
    [SerializeField] private string nextSceneName = "Intro"; 

    private bool isTransitioning = false;
    private bool canStart = false; // 연출 도중에 씬이 넘어가는 것 방지

    private void Start()
    {
        CY.Vehicle.SoundManager.Instance?.PlayTitleBgm();
        
        titleRect.localScale = Vector3.one * 5f; 
        titleCanvasGroup.alpha = 0f;
        pressAnyKeyGroup.alpha = 0f;
        
        fadePanel.alpha = 1f; 
        fadePanel.blocksRaycasts = true;
        
        Sequence seq = DOTween.Sequence();
        
        seq.Append(fadePanel.DOFade(0f, 1f).SetEase(Ease.InOutSine));
        
        seq.Append(titleRect.DOScale(Vector3.one, 0.6f).SetEase(Ease.InExpo));
        seq.Join(titleCanvasGroup.DOFade(1f, 0.6f));
        
        seq.AppendCallback(() => 
        {
            shakeContainer.DOShakeAnchorPos(0.4f, strength: new Vector2(30f, 30f), vibrato: 15);
        });
        
        seq.AppendInterval(0.5f);
        seq.Append(pressAnyKeyGroup.DOFade(1f, 0.5f));
        seq.AppendCallback(() => 
        {
            canStart = true;
            pressAnyKeyGroup.DOFade(0f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        });
    }

    private void Update()
    {
        if (canStart && !isTransitioning && Input.anyKeyDown)
        {
            GoToNextScene();
        }
    }

    private void GoToNextScene()
    {
        isTransitioning = true;
        
        DOTween.Kill(pressAnyKeyGroup);
        
        fadePanel.blocksRaycasts = true;
        fadePanel.DOFade(1f, 1f).SetEase(Ease.InOutSine).OnComplete(() => 
        {
            SceneManager.LoadScene(nextSceneName);
        });
    }
}
