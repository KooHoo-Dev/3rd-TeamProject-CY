using Cysharp.Threading.Tasks;
using HelloServer;
using UnityEngine;

namespace CY.Network
{
    public class VehicleStateSender: MonoBehaviour
    {
        [SerializeField] private Rigidbody2D body;
        [SerializeField] private float sendPerSecond = 15f;
        
        private NetworkSession session;
        private float nextSendAt;

        private void Awake()
        {
            session = NetworkSession.Instance;
            if (body == null) body = GetComponent<Rigidbody2D>();
        }
        
        private void FixedUpdate()
        {
            if (Session.IsHost == false) return;
            if (body == null || session == null) return;
            if (Time.unscaledTime < nextSendAt) return;

            // Max(1f, ) 최소 초당 1번 보장. 음수나 0 들어올 경우 대비.
            nextSendAt = Time.unscaledTime + 1f / Mathf.Max(1f, sendPerSecond);

            session.SendAsync(new VehicleStateMessage
            {
                SceneVersion = session.CurrentSceneVersion,
                PositionX = body.position.x,
                PositionY = body.position.y,
                Rotation = body.rotation,
                VelocityX = body.linearVelocity.x,
                VelocityY = body.linearVelocity.y
            }).Forget();
        }
    }
}