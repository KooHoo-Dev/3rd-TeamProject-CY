using HelloServer;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private string sceneName = null;
    [SerializeField] private RoomClient roomClient;
    
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
