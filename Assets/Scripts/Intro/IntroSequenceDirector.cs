using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Intro
{
    public class IntroSequenceDirector : MonoBehaviour
    {
        [SerializeField] private IntroDialogueController dialogue;
        [SerializeField] private ScreenFadeController fade;
        [SerializeField] private Image FadeImage;
        [SerializeField] private string nextSceneName = "Lobby";
        
        private Coroutine introRoutine;
        private bool isTransitioning;
        
        private void Start()
        {
            StartCoroutine(PlayIntro());
        }
        
        private IEnumerator PlayIntro()
        {
            FadeImage.gameObject.SetActive(true);
            yield return fade.FadeIn(1f);
            yield return dialogue.PlayGroup("opening");
            yield return fade.FadeOut(Color.black, 1f);

            SceneManager.LoadScene(nextSceneName);
        }
        
        public void SkipIntro()
        {
            if (isTransitioning) return;
            if (introRoutine != null) StopCoroutine(introRoutine);

            StartCoroutine(TransitionToNextScene());
        }
        
        private IEnumerator TransitionToNextScene()
        {
            if (isTransitioning) yield break;
            isTransitioning = true;
            
            yield return fade.FadeOut(Color.black, 0.5f);

            SceneManager.LoadScene(nextSceneName);
        }
    }
}