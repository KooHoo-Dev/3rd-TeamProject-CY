using CY.Player;
using CY.Vehicle;
using UnityEngine;

namespace CY.Network
{
    public enum VehicleInputMode { Network = 0, LocalTest = 1 }
    
    public class VehicleInputInstaller: MonoBehaviour
    {
        [SerializeField] private VehicleInputMode mode = VehicleInputMode.Network;

        [Header("대상 (비워두면 같은 오브젝트에서 찾는다)")]
        [SerializeField] private VehicleController controller;
        [SerializeField] private LocalKeyboardInputSource localInput;
        [SerializeField] private NetworkInputSource networkInput;

        [Tooltip("로컬 테스트일 때 꺼둘 네트워크 컴포넌트들. 비워두면 같은 오브젝트의 네트워크 컴포넌트를 전부 모은다.")]
        [SerializeField] private MonoBehaviour[] networkOnly;

        [SerializeField] private VehicleRole localTestRole = VehicleRole.Handle;

        private void Awake()
        {
            if (controller == null) controller = GetComponent<VehicleController>();
            if (localInput == null) localInput = GetComponent<LocalKeyboardInputSource>();
            if (networkInput == null) networkInput = GetComponent<NetworkInputSource>();

            if (networkOnly == null || networkOnly.Length == 0)
                networkOnly = CollectNetworkOnly();

            bool isLocalTest = mode == VehicleInputMode.LocalTest;

            foreach (MonoBehaviour behaviour in networkOnly)
                if (behaviour != null) behaviour.enabled = (isLocalTest == false);
            
            if (localInput != null) localInput.SetReadAllRoles(isLocalTest);

            if (controller != null)
                controller.SetInputSource(isLocalTest ? localInput : (IVehicleInputSource)networkInput);

            // 로컬 테스트는 호스트로 고정해야 라운드·타이머·경유지가 돈다
            if (isLocalTest) Session.Apply(true, localTestRole);
        }

        private MonoBehaviour[] CollectNetworkOnly()
        {
            return new MonoBehaviour[]
            {
                GetComponent<VehicleRoleAssigner>(),
                GetComponent<VehicleInputSender>(),
                GetComponent<VehicleStateSender>(),
                GetComponent<VehicleStateReceiver>(),
                networkInput
            };
        }
    }
}