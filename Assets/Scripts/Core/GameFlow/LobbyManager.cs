using HelloServer;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private string sceneName = null;
    [SerializeField] private RoomClient roomClient;
    [SerializeField] private Button gameStartBtn;

    private void Update()
    {
        gameStartBtn.interactable = roomClient.RemoteCount == 3;
    }
    
    public void OnGameStartButtonClick()
    {
        if (roomClient == null)
        {
            Debug.Log("RoomClient가 연결되지 않았습니다.");
            return;
        }

        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.Log("이동할 씬 이름이 비어있습니다.");
            return;
        }
        
        roomClient.RequestSceneChange(sceneName);
    }
}
