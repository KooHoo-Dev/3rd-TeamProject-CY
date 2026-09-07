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
    }
}