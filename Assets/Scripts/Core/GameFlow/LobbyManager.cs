using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private string sceneName = "GameScene";
    
    public void OnGameStartButtonClick()
    {
        Debug.Log($"이동할 씬 명 : {sceneName}");
    }
}
