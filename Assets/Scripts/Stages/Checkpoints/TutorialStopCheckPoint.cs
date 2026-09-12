using CY.Network;
using CY.Vehicle;
using UnityEngine;

namespace CY.Stages
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class TutorialStopCheckpoint : MonoBehaviour
    {
        [SerializeField] private float stopSpeedThreshold = 0.05f;
        [SerializeField] private RoundStateSync roundStateSync;

        public bool IsCompleted => completed;

        private BoxCollider2D zone;
        private Rigidbody2D vehicleBody;
        private VehicleController vehicleController;
        private BoxCollider2D vehicleBox;
        private bool completed;

        private void Reset()
        {
            GetComponent<BoxCollider2D>().isTrigger = true;
        }

        private void Awake()
        {
            zone = GetComponent<BoxCollider2D>();
            zone.isTrigger = true;

            if (roundStateSync == null)
                roundStateSync = FindAnyObjectByType<RoundStateSync>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (completed) return;

            Rigidbody2D body = other.attachedRigidbody;
            if (body == null) return;

            VehicleController controller = body.GetComponent<VehicleController>();
            if (controller == null) return;

            vehicleBody = body;
            vehicleController = controller;
            vehicleBox = body.GetComponent<BoxCollider2D>();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.attachedRigidbody != vehicleBody) return;

            vehicleBody = null;
            vehicleController = null;
            vehicleBox = null;
        }

        private void Update()
        {
            if (Session.IsHost == false) return;
            if (completed) return;
            if (vehicleBody == null || vehicleController == null) return;

            if (vehicleController.LastInput.CurrentGear != Gear.N) return;
            if (vehicleBody.linearVelocity.magnitude > stopSpeedThreshold) return;
            if (IsFullyInside() == false) return;

            completed = true;

            // 기존 round_state 패킷을 통해 다른 클라이언트에도 완료 상태를 보낸다.
            roundStateSync?.SetTutorialStopCompleted(true);

            Debug.Log("[Tutorial] N 기어 정차 체크포인트 완료", this);
        }

        private bool IsFullyInside()
        {
            if (vehicleBox == null) return false;

            Vector2 offset = vehicleBox.offset;
            Vector2 halfSize = vehicleBox.size * 0.5f;

            return IsCornerInside(offset + new Vector2(halfSize.x, halfSize.y))
                   && IsCornerInside(offset + new Vector2(halfSize.x, -halfSize.y))
                   && IsCornerInside(offset + new Vector2(-halfSize.x, halfSize.y))
                   && IsCornerInside(offset + new Vector2(-halfSize.x, -halfSize.y));
        }

        private bool IsCornerInside(Vector2 localCorner)
        {
            Vector2 world = vehicleBox.transform.TransformPoint(localCorner);
            return zone.OverlapPoint(world);
        }
    }
}