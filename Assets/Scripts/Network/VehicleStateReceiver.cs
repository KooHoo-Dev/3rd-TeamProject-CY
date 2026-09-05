using System.Collections.Generic;
using CY.Player;
using CY.Vehicle;
using HelloServer;
using Newtonsoft.Json;
using UnityEngine;

namespace CY.Network
{
    public struct VehicleSnapshot
    {
        public Vector2 Position;
        public float Rotation;
        public Vector2 Velocity;
    }

    public class VehicleStateReceiver : MonoBehaviour
    {
        [SerializeField] private VehicleRoleAssigner roleAssigner;
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private float positionSmoothTime = 0.08f;
        [SerializeField] private float rotationSmoothSpeed = 720f;
        
        [Tooltip("이만큼 벌어지면 보간 없이 즉시 옮긴다. 재시작 리스폰이나 통신 지연으로 크게 벌어졌을 때.")]
        [SerializeField] private float snapDistance = 5f;
        
        private NetworkSession session;
        private Vector2 targetPosition;
        private float targetRotation;
        private Vector2 smoothVelocity;
        private bool hasSnapshot;

        public void ApplySnapshot(VehicleSnapshot snapshot)
        {
            targetPosition = snapshot.Position;
            targetRotation = snapshot.Rotation;
            hasSnapshot = true;
        }

        private void Awake()
        {
            if (body == null) body = GetComponent<Rigidbody2D>();
            if (roleAssigner == null) roleAssigner = GetComponent<VehicleRoleAssigner>();
        }
        
        private void OnEnable()
        {
            session = NetworkSession.Instance;
            if (session != null) session.MessageReceived += HandleMessage;
            if (roleAssigner != null) roleAssigner.RolesChanged += OnRolesChanged;
        }

        private void OnDisable()
        {
            if (session != null) session.MessageReceived -= HandleMessage;
            if (roleAssigner != null) roleAssigner.RolesChanged -= OnRolesChanged;
        }

        private void Update()
        {
            if (Session.IsHost || hasSnapshot == false) return;
            
            if (Vector2.Distance(transform.position, targetPosition) > snapDistance)
            {
                transform.position = targetPosition;
                transform.rotation = Quaternion.Euler(0f, 0f, targetRotation);
                smoothVelocity = Vector2.zero;
                return;
            }
            
            transform.position = Vector2.SmoothDamp(transform.position, targetPosition, ref smoothVelocity, positionSmoothTime);
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.z, targetRotation, rotationSmoothSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        
        private void OnRolesChanged(IReadOnlyDictionary<string, VehicleRole> _)
        {
            if (body != null) body.simulated = Session.IsHost;
            if (Session.IsHost) hasSnapshot = false;
        }
        
        private void HandleMessage(string type, string json)
        {
            if (type != "vehicle_state" || Session.IsHost) return;

            VehicleStateMessage message = JsonConvert.DeserializeObject<VehicleStateMessage>(json);
            if (message == null) return;
            
            ApplySnapshot(new VehicleSnapshot
            {
                Position = new Vector2(message.PositionX, message.PositionY),
                Rotation = message.Rotation,
                Velocity = new Vector2(message.VelocityX, message.VelocityY)
            });
        }
    }
}