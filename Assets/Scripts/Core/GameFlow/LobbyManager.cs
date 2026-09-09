using HelloServer;
using UnityEngine;
using UnityEngine.UI;
using CY.Vehicle;

namespace CY.Core
{
    public class LobbyManager : MonoBehaviour
    {
        [SerializeField] private string sceneName = null;
        [SerializeField] private RoomClient roomClient;
        [SerializeField] private Button gameStartBtn;
        
        private void Start()
        {
            // 로비 씬에 진입하면 로비 BGM 재생
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayLobbyBgm();
            }
        }

        private void Update()
        {
            gameStartBtn.interactable = roomClient.RemoteCount == 3;
        }
    
        public void OnGameStartButtonClick()
        {
            // 클릭하자마자 최우선으로 로비 BGM을 즉시 정지
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayUIClick();
            }
            
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
        
        public void OnGameEndButtonClick()
        {
            SoundManager.Instance?.PlayUIClick();

        #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        }
    }
}
