using System.Collections;
using CY.Vehicle;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace CY.UI
{
    public class UIClickSound : MonoBehaviour
    {
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RegisterAllButtons();
        }

        // [핵심 변경] 코루틴을 사용해 자동 스캔 시작
        private IEnumerator Start()
        {
            RegisterAllButtons(); // 처음에 한 번 싹 등록

            // 1.5초마다 씬을 검사해서 새로 생성된(Instantiate) 버튼을 찾아냅니다.
            WaitForSeconds wait = new WaitForSeconds(1.5f);
            
            while (true)
            {
                yield return wait;
                RegisterAllButtons(); 
            }
        }

        public void RegisterAllButtons()
        {
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (Button btn in allButtons)
            {
                // 중복 등록 방지를 위해 기존 리스너 제거 후 추가 (매우 안전함)
                btn.onClick.RemoveListener(OnAnyButtonClicked);
                btn.onClick.AddListener(OnAnyButtonClicked);
            }
        }

        private void OnAnyButtonClicked()
        {
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayUIClick();
            }
        }
    }
}