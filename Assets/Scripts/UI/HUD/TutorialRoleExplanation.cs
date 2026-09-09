using System.Collections.Generic;
using CY.Network;
using CY.Player;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace CY.UI
{
    public class TutorialRoleExplanation : MonoBehaviour
    {
        [Header("Child Objects")]
        [SerializeField] private GameObject handleExplanation;
        [SerializeField] private GameObject accelExplanation;
        [SerializeField] private GameObject brakeExplanation;
        [SerializeField] private GameObject gearExplanation;

        [Header("Brake Explain Phase")]
        [SerializeField] private GameObject brakeFirst;
        [SerializeField] private GameObject brakeSecond;
        
        [Header("buttons")]
        [SerializeField] private Button continueButton;
        [SerializeField] private RectTransform noticePanel;
        [SerializeField] private float hideOffsetY = 900f;
        [SerializeField] private float hideDuration = 0.35f;
        
        [Header("Role Assigner")]
        [SerializeField] private VehicleRoleAssigner roleAssigner;
        
        private VehicleRole currentRole = VehicleRole.None;
        private bool isBrakeSecondPage;
        private bool isClosing;
        
        private void Start()
        {
            if (roleAssigner == null)
                roleAssigner = FindAnyObjectByType<VehicleRoleAssigner>();

            // 역할이 이후 다시 계산되는 경우에도 안내문을 갱신한다.
            if (roleAssigner != null)
                roleAssigner.RolesChanged += OnRolesChanged;

            continueButton.onClick.AddListener(OnContinueButtonClicked);
            
            RefreshExplanation();
        }
        
        private void OnDestroy()
        {
            if (roleAssigner != null)
                roleAssigner.RolesChanged -= OnRolesChanged;

            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnContinueButtonClicked);

            if (noticePanel != null)
                noticePanel.DOKill();
        }

        private void OnRolesChanged(IReadOnlyDictionary<string, VehicleRole> _)
        {
            RefreshExplanation();
        }
        
        private void RefreshExplanation()
        {
            currentRole = Session.MyRole;

            handleExplanation.SetActive(currentRole == VehicleRole.Handle);
            accelExplanation.SetActive(currentRole == VehicleRole.Accel);
            brakeExplanation.SetActive(currentRole == VehicleRole.Brake);
            gearExplanation.SetActive(currentRole == VehicleRole.Gear);

            if (currentRole == VehicleRole.Brake)
            {
                isBrakeSecondPage = false;
                brakeFirst.SetActive(true);
                brakeSecond.SetActive(false);
            }

            continueButton.interactable = Session.HasAssignedRole;
        }
        
        private void OnContinueButtonClicked()
        {
            if (isClosing || !Session.HasAssignedRole)
                return;

            // 브레이크 역할의 첫 클릭만 두 번째 설명으로 전환한다.
            if (currentRole == VehicleRole.Brake && !isBrakeSecondPage)
            {
                isBrakeSecondPage = true;
                brakeFirst.SetActive(false);
                brakeSecond.SetActive(true);
                return;
            }

            HideNoticePanel();
        }
        
        private void HideNoticePanel()
        {
            isClosing = true;
            continueButton.interactable = false;

            noticePanel.DOKill();
            noticePanel
                .DOAnchorPosY(
                    noticePanel.anchoredPosition.y + hideOffsetY,
                    hideDuration)
                .SetEase(Ease.InCubic)
                .OnComplete(() => noticePanel.gameObject.SetActive(false));
        }
    }

}