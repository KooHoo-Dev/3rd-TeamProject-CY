using CY.Player;
using CY.Vehicle;
using HelloServer;
using Newtonsoft.Json;
using UnityEngine;

namespace CY.Network
{
    public class NetworkInputSource: MonoBehaviour, IVehicleInputSource
    {
        [SerializeField] private VehicleRoleAssigner roleAssigner;
        [SerializeField] private LocalKeyboardInputSource localInput;
        
        [Tooltip("이 시간 동안 입력이 안 오면 그 역할을 비운다")] // 덮어쓰는 형식으로 되어있으므로 유령값이 계속 남아있을 수 있음
        [SerializeField] private float inputTimeout = 0.25f;

        private readonly VehicleInputMerger merger = new();
        private readonly float[] lastReceivedAt = new float[4];

        private NetworkSession session;
        
        private void Awake()
        {
            session = NetworkSession.Instance;

            if (roleAssigner == null) roleAssigner = GetComponent<VehicleRoleAssigner>();
            if (localInput == null) localInput = GetComponent<LocalKeyboardInputSource>();
        }
        
        private void OnEnable()
        {
            if (session != null) session.MessageReceived += HandleMessage;
        }

        private void OnDisable()
        {
            if (session != null) session.MessageReceived -= HandleMessage;
        }
        
        private void Update()
        {
            // 호스트 자기 입력은 서버를 왕복시키지 않고 바로 넣는다
            if (Session.IsHost && Session.HasAssignedRole && localInput != null)
                Set(Session.MyRole, localInput.Read());
        }
        
        public VehicleInput Read()
        {
            if (Session.IsHost == false) return default;

            ClearTimedOut();
            return merger.Merge();
        }
        
        private void HandleMessage(string type, string json)
        {
            if (type != "vehicle_input") return;
            if (Session.IsHost == false) return;

            VehicleInputMessage message = JsonConvert.DeserializeObject<VehicleInputMessage>(json);
            if (message == null) return;
            
            // 내가 보낸 게 되돌아온 것은 버린다. 최신값은 Update 에서 직접 넣는다.
            if (session != null && session.Me != null && message.UserId == session.Me.Id) return;

            if (roleAssigner == null) return;
            if (roleAssigner.TryGetRole(message.UserId, out VehicleRole role) == false) return;

            Set(role, new VehicleInput
            {
                Steer = Mathf.Clamp(message.Steer, -1f, 1f),
                Accel = Mathf.Clamp01(message.Accel),
                Brake = Mathf.Clamp01(message.Brake),
                CurrentGear = ToGear(message.Gear)
            });
        }
        
        private void Set(VehicleRole role, VehicleInput input)
        {
            int index = (int)role;
            if (index < 0 || index >= lastReceivedAt.Length) return;

            merger.Set(role, input);
            lastReceivedAt[(int)role] = Time.unscaledTime;
        }
        
        private void ClearTimedOut()
        {
            for (int i = 0; i < lastReceivedAt.Length; i++)
            {
                if (Time.unscaledTime - lastReceivedAt[i] <= inputTimeout) continue;

                merger.Set((VehicleRole)i, default);
            }
        }

        private static Gear ToGear(int value)
        {
            return value switch
            {
                1 => Gear.D,
                2 => Gear.R,
                _ => Gear.N
            };
        }
    }
}