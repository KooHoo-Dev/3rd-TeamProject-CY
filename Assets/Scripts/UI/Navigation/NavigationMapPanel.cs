using CY.Network;
using CY.Player;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace CY.UI
{
    public class NavigationMapPanel: MonoBehaviour
    {
        [Header("슬라이드")]
        [SerializeField] private RectTransform panel;
        [SerializeField] private float hiddenX   = 1400f;   // 화면 오른쪽 바깥
        [SerializeField] private float shownX    = 0f;
        [SerializeField] private float slideTime = 0.18f;
        
        [Header("어두운 배경")]
        [SerializeField] private Image dimmer;
        [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.8f;
        [SerializeField] private float fadeTime = 0.18f;
        
        [Header("역할 제한")]
        [SerializeField] private bool ignoreRoleCheck = true; // TMP : 네트워크 붙인 후엔 제거
        
        public bool IsOpen => isOpen;

        private bool  isOpen;
        private float slideVelocity;
        private float dimVelocity;

        private void Start()
        {
            Vector2 pos = panel.anchoredPosition;
            pos.x = hiddenX;
            panel.anchoredPosition = pos;
            
            SetDimAlpha(0f);
        }

        private void Update()
        {
            // TMP : 네트워크 붙인 후엔 !ignoreRoleCheck 조건 제거
            if (!ignoreRoleCheck && Session.MyRole != VehicleRole.Gear) return;
            
            Keyboard kb = Keyboard.current;
            if (kb != null && kb.tabKey.wasPressedThisFrame) isOpen = !isOpen;

            // 패널 슬라이드
            float targetX = isOpen ? shownX : hiddenX;
            Vector2 pos = panel.anchoredPosition;
            pos.x = Mathf.SmoothDamp(pos.x, targetX, ref slideVelocity, slideTime);
            panel.anchoredPosition = pos;
            
            // 배경 어둡게
            if (dimmer != null)
            {
                float targetDimAlpha = isOpen ? dimAlpha : 0f;
                SetDimAlpha(Mathf.SmoothDamp(dimmer.color.a, targetDimAlpha, ref dimVelocity, fadeTime));
            }
        }
        
        private void SetDimAlpha(float a)
        {
            Color c = dimmer.color;
            c.a = a;
            dimmer.color = c;
        }
    }
}