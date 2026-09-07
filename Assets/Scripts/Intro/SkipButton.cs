using Intro;
using UnityEngine;

public class SkipButton : MonoBehaviour
{
    [SerializeField] private IntroSequenceDirector introSequenceDirector;

    public void OnSkipButtonPressed()
    {
        introSequenceDirector.SkipIntro();
    }
}
