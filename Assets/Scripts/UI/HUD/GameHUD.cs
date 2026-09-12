using CY.Core;
using CY.Network;
using CY.Player;
using CY.Vehicle;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CY.UI
{
    public class GameHud : MonoBehaviour
    {
        [Header("상태 공급자")]
        [SerializeField] private HostRoundStatus hostStatus;
        [SerializeField] private NetworkRoundStatus networkStatus;
        
        // 상단
        
        [Header("역할")]
        [SerializeField] private TMP_Text roleText;
        
        [Header("제한 시간")]
        [SerializeField] private TMP_Text timeText;

        [Header("내구도")]
        [SerializeField] private RectTransform durabilityGroup;
        [SerializeField] private Image durabilityFill;
        [SerializeField] private TMP_Text durabilityPercent;
        [SerializeField] private GameObject warningIcon;
        [SerializeField] private Color healthyColor = new Color(0.35f, 0.85f, 0.40f);
        [SerializeField] private Color damagedColor = new Color(0.95f, 0.80f, 0.25f);
        [SerializeField] private Color severeColor  = new Color(0.90f, 0.30f, 0.25f);
        [SerializeField, Range(0f, 1f)] private float damagedThreshold = 0.4f;
        [SerializeField, Range(0f, 1f)] private float severeThreshold  = 0.2f;
        [SerializeField] private float fillSmoothTime = 0.18f;
        [SerializeField] private float shakeAmplitude = 3f;
        [SerializeField] private float shakeSpeed     = 18f;
        
        // 하단
        
        [Header("미션")]
        [SerializeField] private TMP_Text missionText;
        
        [Header("기어")]
        [SerializeField] private TMP_Text gearText;
        [SerializeField] private Color gearNormalColor  = Color.white;
        [SerializeField] private Color gearReverseColor = new Color(1f, 0.5f, 0.3f);
        
        [Header("속도")]
        [SerializeField] private Image speedFill;
        [SerializeField] private TMP_Text speedText;
        [SerializeField] private float topSpeedDisplay = 120f;   // 최고속일 때 화면에 뜰 숫자
        [SerializeField] private float speedSmoothTime = 0.1f;

        private float shownRatio;
        private float speedVelocity;

        private IRoundStatusProvider Status =>
            Session.IsHost ? (IRoundStatusProvider)hostStatus : networkStatus;

        private float shownFill = 1f;
        private float fillVelocity;

        private Vector2 durabilityBasePos;

        private void Awake()
        {
            if (durabilityGroup != null) durabilityBasePos = durabilityGroup.anchoredPosition;
            
            if (hostStatus == null) hostStatus = FindAnyObjectByType<HostRoundStatus>();
            if (networkStatus == null) networkStatus = FindAnyObjectByType<NetworkRoundStatus>();
        }

        private void Start()
        {
            IRoundStatusProvider s = Status;
            shownFill = s != null ? s.DurabilityRatio : 1f;
        }

        private void Update()
        {
            IRoundStatusProvider s = Status;
            if (s == null) return;

            UpdateRole();
            UpdateTime(s);
            UpdateDurability(s);
            UpdateMission(s);
            UpdateSpeed(s);
            UpdateGear(s);
        }

        private void UpdateRole()
        {
            if (roleText == null) return;
            roleText.text = $"[{RoleName(Session.MyRole)}]";
        }

        private static string RoleName(VehicleRole role) => role switch
        {
            VehicleRole.Handle => "핸들",
            VehicleRole.Accel => "액셀",
            VehicleRole.Brake => "브레이크·내비게이션",
            VehicleRole.Gear => "기어",
            _ => "대기중",
        };

        private void UpdateTime(IRoundStatusProvider s)
        {
            if (timeText == null) return;
        
            int total = Mathf.FloorToInt(s.ElapsedTime);
            timeText.text = $"{total / 60:00}:{total % 60:00}";
        }

        private void UpdateDurability(IRoundStatusProvider s)
        {
            float ratio = Mathf.Clamp01(s.DurabilityRatio);

            if (durabilityFill != null)
            {
                shownFill = Mathf.SmoothDamp(shownFill, ratio, ref fillVelocity, fillSmoothTime);
                durabilityFill.fillAmount = shownFill;

                durabilityFill.color =
                    ratio > damagedThreshold ? healthyColor :
                    ratio > severeThreshold  ? damagedColor :
                                               severeColor;
            }

            if (durabilityPercent != null)
                durabilityPercent.text = $"{Mathf.RoundToInt(ratio * 100f)}%";

            bool severe = ratio <= severeThreshold;
            if (warningIcon != null && warningIcon.activeSelf != severe)
                warningIcon.SetActive(severe);

            // 20% 이하에서 살짝 흔들림
            if (durabilityGroup != null)
            {
                Vector2 offset = Vector2.zero;
                if (severe)
                {
                    float t = Time.unscaledTime * shakeSpeed;
                    offset = new Vector2(
                        (Mathf.PerlinNoise(t, 0f) - 0.5f) * 2f,
                        (Mathf.PerlinNoise(0f, t) - 0.5f) * 2f) * shakeAmplitude;
                }
                durabilityGroup.anchoredPosition = durabilityBasePos + offset;
            }
        }
        
        private void UpdateMission(IRoundStatusProvider s)
        {
            if (missionText == null) return;
            missionText.text = $"미션  {s.ClearedCheckpoints} / {s.TotalCheckpoints}";
        }
        
        private void UpdateSpeed(IRoundStatusProvider s)
        {
            shownRatio = Mathf.SmoothDamp(shownRatio, s.SpeedRatio, ref speedVelocity, speedSmoothTime);

            if (speedFill != null)
                speedFill.fillAmount = shownRatio;

            if (speedText != null)
                speedText.text = $"{Mathf.RoundToInt(shownRatio * topSpeedDisplay)}";
        }
        
        private void UpdateGear(IRoundStatusProvider s)
        {
            if (gearText == null) return;
            gearText.text = s.CurrentGear.ToString();      // D / N / R
            gearText.color = s.CurrentGear == Gear.R ? gearReverseColor : gearNormalColor;
        }
    }
}