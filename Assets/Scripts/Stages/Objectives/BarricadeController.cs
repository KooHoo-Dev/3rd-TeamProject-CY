using CY.Minigame;
using UnityEngine;

public class BarricadeController : MonoBehaviour
{
    [SerializeField] private BarricadeMinigameController barricadeMinigameController;
    [SerializeField] GameObject barricadeOpened;
    [SerializeField] GameObject barricadeClosed;

    private void OnEnable()
    {
        barricadeMinigameController.MinigameEnded += HandleMinigameEnded;
    }

    private void OnDisable()
    {
        barricadeMinigameController.MinigameEnded -= HandleMinigameEnded;
    }
    
    public void HandleMinigameEnded()
    {
        barricadeOpened.SetActive(true);
        barricadeClosed.SetActive(false);
    }
}
