using CY.Player;
using CY.Vehicle;
using Cysharp.Threading.Tasks;
using HelloServer;
using UnityEngine;

namespace CY.Network
{
    public class VehicleInputSender : MonoBehaviour
    {
        [SerializeField] private LocalKeyboardInputSource localInput;
        [SerializeField] private float sendPerSecond = 20f;

        private NetworkSession session;
        private float nextSendAt;
        
        private void Awake()
        {
            session = NetworkSession.Instance;
            if (localInput == null) localInput = GetComponent<LocalKeyboardInputSource>();
        }
        
        private void Update()
        {
            if (localInput == null || session == null) return;
            if (Session.HasAssignedRole == false) return;
            // 호스트는 자기 입력을 직접 Merger 에 넣으므로 보낼 필요가 없다
            if (Session.IsHost) return;
            if (Time.unscaledTime < nextSendAt) return;

            nextSendAt = Time.unscaledTime + 1f / Mathf.Max(1f, sendPerSecond);

            VehicleInput input = localInput.Read();

            session.SendAsync(new VehicleInputMessage
            {
                Steer = input.Steer,
                Accel = input.Accel,
                Brake = input.Brake,
                Gear = (int)input.CurrentGear
            }).Forget();
        }
    }
}