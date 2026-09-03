using System;
using System.Collections.Generic;
using System.Linq;
using CY.Player;
using CY.Vehicle;
using Cysharp.Threading.Tasks;
using HelloServer;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

namespace  CY.Network
{
    public class VehicleSyncManager : MonoBehaviour
    {
        [Header("차량 참조")] 
        [SerializeField] private LocalKeyboardInputSource localInput;
        [SerializeField] private Rigidbody2D vehicleBody;
        [SerializeField] private VehicleStateReceiver stateReceiver;

        [Header("전송 주기")] 
        [SerializeField] private float inputSendPerSecond = 20f;
        [SerializeField] private float stateSendPerSecond = 15f;
        
        [Header("입력 끊김")]
        [SerializeField] private float inputTimeout = 0.25f;

        [SerializeField] private TMP_Text roleText;
        
        private readonly Dictionary<string, VehicleRole> roles = new();
        private readonly VehicleInputMerger inputMerger = new();
        private readonly float[] lastInputReceivedAt = new float[4];
        
        private NetworkSession session;

        private float nextInputSendAt;
        private float nextStateSendAt;

        public IReadOnlyDictionary<string, VehicleRole> Roles => roles;
        
        public event Action<IReadOnlyDictionary<string, VehicleRole>> RolesChanged;

        private void Awake()
        {
            session = NetworkSession.Instance;

            if (localInput == null) localInput = GetComponent<LocalKeyboardInputSource>();
            if (vehicleBody == null) vehicleBody = GetComponent<Rigidbody2D>();
            if (stateReceiver == null) stateReceiver = GetComponent<VehicleStateReceiver>();
        }

        private void OnEnable()
        {
            session.MessageReceived += HandleMessage;
            RefreshRoles();
        }
        
        private void OnDisable()
        {
            if (session != null) session.MessageReceived -= HandleMessage;
        }

        private void Update()
        {
            SendLocalInput();
        }

        private void HandleMessage(string type, string json)
        {
            switch (type)
            {
                case "welcome":
                case "join":
                case "leave":
                    RefreshRoles();
                    return;
                
                case "vehicle_input":
                    HandleVehicleInput(JsonConvert.DeserializeObject<VehicleInputMessage>(json));
                    return;
                case "vehicle_state":
                    HandleVehicleState(JsonConvert.DeserializeObject<VehicleStateMessage>(json));
                    return;
            }
        }

        private void RefreshRoles()
        {
            roles.Clear();

            if (session == null || session.Me == null)
            {
                // 서버에 붙은 적이 없으면 기본값(호스트)을 그대로 둔다. 씬을 혼자 재생해도 돌아가야 한다.
                // 붙었다가 끊긴 경우에만 역할을 지운다.
                if (Session.HasAssignedRole) Session.ClearRole();

                RolesChanged?.Invoke(roles);
                return;
            }
            
            List<User> users = session.Participants.Values
                .OrderBy(User => GetJoinOrder(User.Id)).ToList();

            if (users.Count != 4)
            {
                Session.ClearRole();
                
                RolesChanged?.Invoke(roles);
                return;
            }

            List<VehicleRole> shuffledRoles = CreateShuffledRoles(users);

            for (int i = 0; i < users.Count; i++)
            {
                roles[users[i].Id] = shuffledRoles[i];
            }

            if (roles.TryGetValue(session.Me.Id, out VehicleRole myRole))
            {
                bool isHost = myRole == VehicleRole.Handle;
                Session.Apply(isHost, myRole);
            }
            
            UpdateRoleText();
            RolesChanged?.Invoke(roles);
        }

        private void SendLocalInput()
        {
            if (localInput == null) return;
            if (Session.HasAssignedRole == false) return;

            VehicleInput input = localInput.Read();

            if (Session.IsHost) SetInput(Session.MyRole, input);

            if (Time.unscaledTime < nextInputSendAt) return;

            float interval = 1f / Mathf.Max(1f, inputSendPerSecond);
            nextInputSendAt = Time.unscaledTime + interval;

            session.SendAsync(new VehicleInputMessage
            {
                Steer = input.Steer,
                Accel = input.Accel,
                Brake = input.Brake,
                Gear = (int)input.CurrentGear
            }).Forget();
        }

        private void HandleVehicleInput(VehicleInputMessage message)
        {
            if (message == null) return;
            if (Session.IsHost == false) return;

            if (roles.TryGetValue(message.UserId, out VehicleRole role) == false) return;

            SetInput(role, new VehicleInput
            {
                Steer = Mathf.Clamp(message.Steer, -1f, 1f),
                Accel = Mathf.Clamp01(message.Accel),
                Brake = Mathf.Clamp01(message.Brake),
                CurrentGear = ToGear(message.Gear)
            });
        }

        private void HandleVehicleState(VehicleStateMessage message)
        {
            if (message == null) return;
            if (Session.IsHost) return;
            if (stateReceiver == null) return;
            
            if (roles.TryGetValue(message.UserId, out VehicleRole role) == false) return;
            if (role != VehicleRole.Handle) return;
            
            stateReceiver.ApplySnapshot(new VehicleSnapshot
            {
                Position = new Vector2(message.PositionX, message.PositionY),
                Rotation = message.Rotation,
                Velocity = new Vector2(message.VelocityX, message.VelocityY)
            });
        }

        public VehicleInput ReadMergedInput()
        {
            if (Session.IsHost == false) return default;
            
            ClearTimedOutInputs();
            return inputMerger.Merge();
        }

        public void PublishVehicleState(Rigidbody2D body)
        {
            if (Session.IsHost == false) return;
            if (body == null) return;

            if (Time.unscaledTime < nextStateSendAt) return;

            float interval = 1f / Mathf.Max(1f, stateSendPerSecond);
            nextStateSendAt = Time.unscaledTime + interval;
            
            session.SendAsync(new VehicleStateMessage
            {
                PositionX = body.position.x,
                PositionY = body.position.y,
                Rotation = body.rotation,
                VelocityX = body.linearVelocity.x,
                VelocityY = body.linearVelocity.y
            }).Forget();
        }

        private void SetInput(VehicleRole role, VehicleInput input)
        {
            if (role < VehicleRole.Handle || role > VehicleRole.Gear) return;
            
            inputMerger.Set(role, input);
            lastInputReceivedAt[(int)role] = Time.unscaledTime;
        }

        private void ClearTimedOutInputs()
        {
            for (int i = 0; i < lastInputReceivedAt.Length; i++)
            {
                if (Time.unscaledTime - lastInputReceivedAt[i] <= inputTimeout) continue;
                
                inputMerger.Set((VehicleRole)i, default);
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
        
        private static int GetJoinOrder(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length < 2)
                return int.MaxValue;

            return int.TryParse(id.Substring(1), out int order)
                ? order
                : int.MaxValue;
        }
        
        private void UpdateRoleText()
        {
            if (roleText == null) return;
            
            roleText.text = "역할 : " + Session.MyRole switch
            {
                VehicleRole.Handle => "핸들",
                VehicleRole.Accel => "액셀",
                VehicleRole.Brake => "브레이크",
                VehicleRole.Gear => "기어·내비게이션",
                _ => "역할 배정 대기 중"
            };
        }
        
        private List<VehicleRole> CreateShuffledRoles(List<User> users)
        {
            List<VehicleRole> shuffledRoles = new()
            {
                VehicleRole.Handle,
                VehicleRole.Accel,
                VehicleRole.Brake,
                VehicleRole.Gear
            };

            // 모든 클라이언트가 동일하게 갖는 룸 코드 값으로 시드 생성
            uint state = CreateSeed(session.RoomCode, users);

            // 같은 방 코드 + 같은 4명 = 모든 클라이언트에서 같은 결과
            for (int i = shuffledRoles.Count - 1; i > 0; i--)
            {
                state = state * 1664525u + 1013904223u;

                int swapIndex = (int)(state % (uint)(i + 1));

                (shuffledRoles[i], shuffledRoles[swapIndex]) =
                    (shuffledRoles[swapIndex], shuffledRoles[i]);
            }

            return shuffledRoles;
        }
        
        #region 랜덤 시드 생성

        private static uint CreateSeed(string roomCode, List<User> users)
        {
            uint hash = 2166136261u;

            AddToHash(ref hash, roomCode);

            // users는 이미 접속 순서(ID 숫자순)로 정렬된 상태
            foreach (User user in users)
                AddToHash(ref hash, user.Id);

            return hash;
        }

        private static void AddToHash(ref uint hash, string value)
        {
            if (string.IsNullOrEmpty(value))
                return;

            foreach (char character in value)
            {
                hash ^= character;
                hash *= 16777619u;
            }
        }

        #endregion
    }
}













