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
        [SerializeField] private CY.Vehicle.Vehicle targetVehicle; // [추가] 이벤트 발화를 위한 Vehicle 참조
        
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
            if (targetVehicle == null) targetVehicle = GetComponent<CY.Vehicle.Vehicle>(); // [추가] 자동 가져오기
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
            {
                VehicleInput readInput = localInput.Read();
                // [디버그] 읽어온 입력값 확인
                if (readInput.Accel > 0f || readInput.Brake > 0f)
                {
                    Debug.Log($"[NetInputCheck] LocalInput 감지 - Accel: {readInput.Accel}, Brake: {readInput.Brake}");
                }
                Set(Session.MyRole, readInput);
            }
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
            
            // [디버그] 메시지 수신 확인 및 IsHost 상태 체크
            Debug.Log($"[NetInputCheck] vehicle_input 수신됨 / IsHost: {Session.IsHost}");
            
            if (Session.IsHost == false) return;

            VehicleInputMessage message = JsonConvert.DeserializeObject<VehicleInputMessage>(json);
            if (message == null) return;
            
            // 내가 보낸 게 되돌아온 것은 버린다. 최신값은 Update 에서 직접 넣는다.
            if (session != null && session.Me != null && message.UserId == session.Me.Id) return;

            if (roleAssigner == null) return;
            if (roleAssigner.TryGetRole(message.UserId, out VehicleRole role) == false) return;

            VehicleInput input = new VehicleInput
            {
                Steer = Mathf.Clamp(message.Steer, -1f, 1f),
                Accel = Mathf.Clamp01(message.Accel),
                Brake = Mathf.Clamp01(message.Brake),
                CurrentGear = ToGear(message.Gear)
            };

            // 1. 호스트일 때만 호스트용 물리 입력 merger에 반영
            if (Session.IsHost)
            {
                Set(role, input);
            }

            // 2. [핵심] 호스트든 게스트든 관계없이 네트워크로 넘어온 입력 데이터를 Vehicle.cs에 전달하여 소리를 냄!
            if (targetVehicle != null && targetVehicle.Merger != null)
            {
                targetVehicle.Merger.Set(role, input);
            }
        }
        
        private void Set(VehicleRole role, VehicleInput input)
        {
            int index = (int)role;
            if (index < 0 || index >= lastReceivedAt.Length) return;

            merger.Set(role, input);
            
            // Vehicle.cs의 Merger에도 똑같이 입력을 전달하여 오디오 이벤트가 터지도록 유도
            if (targetVehicle != null && targetVehicle.Merger != null)
            {
                targetVehicle.Merger.Set(role, input);
            }
            
            lastReceivedAt[(int)role] = Time.unscaledTime;
        }
        
        private void ClearTimedOut()
        {
            for (int i = 0; i < lastReceivedAt.Length; i++)
            {
                // 기어는 누르는 입력이 아니라 상태라서, 끊겨도 마지막 값을 유지한다
                if ((VehicleRole)i == VehicleRole.Gear) continue;
                
                if (Time.unscaledTime - lastReceivedAt[i] <= inputTimeout) continue;

                merger.Set((VehicleRole)i, default);
                
                // 타임아웃 시 Vehicle.cs의 입력도 함께 비워줌 (사운드 중지용)
                if (targetVehicle != null && targetVehicle.Merger != null)
                {
                    targetVehicle.Merger.Set((VehicleRole)i, default);
                }
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