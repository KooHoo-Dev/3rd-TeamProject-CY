using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialEndButton : MonoBehaviour
{
    public void TutorialEndButtonPressed()
    {
        SceneManager.LoadScene("PlayTest");
    }
}
