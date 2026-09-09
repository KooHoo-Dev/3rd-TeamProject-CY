using Intro;
using UnityEngine;

public class SkipButton : MonoBehaviour
{
    [SerializeField] private IntroSequenceDirector introSequenceDirector;

    public void OnSkipButtonPressed()
    {
        CY.Vehicle.SoundManager.Instance?.PlayUIClick();
        
        introSequenceDirector.SkipIntro();
    }
}
