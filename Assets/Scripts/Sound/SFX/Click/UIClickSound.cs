using CY.Vehicle;
using UnityEngine;
using UnityEngine.UI;

namespace CY.UI
{
    public class UIClickSound : MonoBehaviour
    {
        private void Start()
        {
            RegisterAllButtons();
        }

        public void RegisterAllButtons()
        {
            // 현재 씬에 활성화/비활성화된 모든 버튼을 수집
            Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (Button btn in allButtons)
            {
                // 중복 등록 방지를 위해 기존 리스너 제거 후 추가
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