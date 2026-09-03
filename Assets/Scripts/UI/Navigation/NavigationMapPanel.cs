using CY.Network;
using CY.Player;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

namespace CY.UI
{
    public class NavigationMapPanel : MonoBehaviour
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

        [Header("맵 카메라")]
        [SerializeField] private Camera mapCamera;

        /// <summary>내비게이션 담당만 지도를 연다. 역할 배정 전(로컬 테스트)에는 누구나 열린다.</summary>
        public bool CanUse =>
            Session.HasAssignedRole == false || Session.MyRole == VehicleRole.Gear;

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

            if (mapCamera != null) mapCamera.enabled = false;
        }

        private void Update()
        {
            if (CanUse == false)
            {
                isOpen = false;
            }
            else
            {
                Keyboard keyboard = Keyboard.current;
                if (keyboard != null && keyboard.tabKey.wasPressedThisFrame) isOpen = !isOpen;
            }

            // 역할과 무관하게 항상 돈다. 중간에 멈추면 패널이 걸린 채로 남는다.
            float targetX = isOpen ? shownX : hiddenX;
            Vector2 pos = panel.anchoredPosition;
            pos.x = Mathf.SmoothDamp(pos.x, targetX, ref slideVelocity, slideTime);
            panel.anchoredPosition = pos;

            if (dimmer != null)
            {
                float targetDimAlpha = isOpen ? dimAlpha : 0f;
                SetDimAlpha(Mathf.SmoothDamp(dimmer.color.a, targetDimAlpha, ref dimVelocity, fadeTime));
            }

            // SmoothDamp 은 목표에 딱 도달하지 않아서 1픽셀 여유를 둔다
            if (mapCamera != null)
            {
                bool needsRender = isOpen || Mathf.Abs(pos.x - hiddenX) > 1f;
                if (mapCamera.enabled != needsRender) mapCamera.enabled = needsRender;
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
