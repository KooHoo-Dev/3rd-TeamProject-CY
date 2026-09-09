using CY.UI;
using CY.Vehicle;
using UnityEngine;

namespace CY.UI
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class TutorialGuideTrigger : MonoBehaviour
    {
        [TextArea]
        [SerializeField] private string guideMessage;

        private BoxCollider2D zone;
        private Transform vehicle;
        private TutorialGuideUI guideUI;
        private bool hasShown;

        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void Awake()
        {
            zone = GetComponent<BoxCollider2D>();
            zone.isTrigger = true;
        }

        private void Start()
        {
            vehicle = FindAnyObjectByType<VehicleController>().transform;
            guideUI = FindAnyObjectByType<TutorialGuideUI>();
        }

        private void Update()
        {
            if (hasShown)
                return;

            if (zone.OverlapPoint(vehicle.position) == false)
                return;

            hasShown = true;
            guideUI.Show(guideMessage);
        }
    }
}